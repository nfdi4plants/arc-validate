# arc-validate

`arc-validate` is a CLI tool for validating ARCs and managing validation packages.
It can run its built-in ARC specification validation or execute an installed
F# or Python validation package.

## Installation

## Command Line Usage

```
USAGE: arc-validate [--help] [--verbose] [<subcommand> [<options>]]

SUBCOMMANDS:

    validate, v <options> command for performing arc validation
    package, p <options>  subcommands for validation packages

    Use 'arc-validate <subcommand> --help' for additional information.

OPTIONS:

    --verbose             Use verbose error messages (with full error stack).
    --help, -h            display this list of options.
```

### The validate command

```
USAGE: arc-validate validate [--help] [--arc-directory <path>]
                             [--out-directory <path>] [--package <package name>]
                             [--package-version <package version>]
                             [--specification-version <specification version>]
                             [--source-branch <branch>]
                             [--source-commit-hash <commit hash>]
                             [--validation-config <path>]
                             [--validation-config-sha256 <sha256>]
                             [-- <package arguments>]

OPTIONS:

    --arc-directory, -i <path>
                          ARC directory to validate. Defaults to ARC_PATH, then
                          to the current directory when ARC_PATH is unset.
    --out-directory, -o <path>
                          Base directory for .arc-validate-results. Defaults to
                          the ARC directory.
    --package, -p <package name>
                          Installed validation package to execute. Without this
                          option, the built-in ARC specification validation runs.
    --package-version, -v <package version>
                          Installed package version to execute. Defaults to the
                          latest installed version.
    --specification-version <specification version>
                          Built-in ARC specification version. This option is
                          ignored when --package is supplied. Defaults to latest.
    --source-branch <branch>
                          Optional source branch recorded in generated outputs.
    --source-commit-hash <commit hash>
                          Optional source commit recorded in generated outputs.
    --validation-config <path>
                          Read configured package inputs from this explicit
                          validation_packages.yml path. Requires --package and
                          --package-version.
    --validation-config-sha256 <sha256>
                          Require the configuration bytes to match this
                          lowercase SHA-256 digest.
    --help, -h            display this list of options.
```

## Validation-package arguments

Arguments on the `validate` command are owned either by arc-validate itself or
by the validation-package execution contract. Package selectors are consumed
by arc-validate and are not forwarded to the package process.

| CLI argument | Package process | Behavior |
| --- | --- | --- |
| `--package`, `-p` | Not forwarded | Selects the installed package. |
| `--package-version`, `-v` | Not forwarded | Selects the installed package version. |
| `--specification-version` | Not forwarded | Selects built-in specification validation and has no package meaning. |
| `--verbose` | Not forwarded | Controls arc-validate diagnostics. |
| `--arc-directory`, `-i` | Always forwarded as `-i <path>` | The resolved ARC path. The forwarded path is absolute and ends in `/`. |
| `--out-directory`, `-o` | Always forwarded as `-o <path>` | The result base directory; defaults to the resolved ARC path. |
| `--source-branch` | Forwarded when supplied | Optional source provenance. |
| `--source-commit-hash` | Forwarded when supplied | Optional source provenance. |
| `--validation-config` | Not forwarded | Selects config-driven execution from an explicit file. Requires package and exact package version selectors. |
| `--validation-config-sha256` | Not forwarded | Verifies the exact configuration bytes before configured values are used. |

The ARC directory, output directory, source branch, and source commit rows are
the standard validation-package arguments. arc-validate starts package scripts
with an argument list rather than a shell command:

```text
dotnet fsi <package.fsx> -i <arc> -o <output> [--source-branch <branch>] [--source-commit-hash <hash>]
uv run <package.py> -i <arc> -o <output> [--source-branch <branch>] [--source-commit-hash <hash>]
```

Values remain separate process arguments, so paths, branch names, and other
values containing spaces do not require package-specific shell escaping. The
source arguments are independent: either, both, or neither may be supplied.

ARCExpect's `PackageArguments.fromCommandLine` API reads all four standard
arguments on .NET, JavaScript, and Python. The .NET and Python validation
pipelines use the parsed output directory and source provenance when supplied
with the resulting `PackageArguments` value.

### Package-defined and CWL arguments

Use the first `--` as the strict boundary between arc-validate options and
package-defined arguments:

```bash
arc-validate validate \
  -p example \
  -i ./my-arc \
  --source-branch dev \
  -- \
  --test \
  --echo "hello world"
```

The boundary is valid only for `validate` with `--package`/`-p`. Standard
arguments stay to its left. arc-validate parses only the left side, strips the
boundary, and appends every token on the right unchanged to the package process
argument list. It never joins those tokens into a shell command.

ARCExpect validates the package side against `ValidationPackageMetadata.Inputs`:

- `inputBinding.prefix` defines the option spelling.
- A boolean prefix is a flag: present means `true`; an absent required boolean
  means `false`; an absent nullable `boolean?` remains optional.
- Non-boolean inputs consume the next process argument as their value.
- Every input requires a unique, non-empty prefix; positional and joined-value
  bindings are not supported.
- A `?` CWL type is optional. Missing non-nullable, non-boolean inputs fail.
- Unknown arguments, duplicates, malformed scalar values, duplicate bindings,
  and collisions with the four standard arguments fail with an actionable
  error.

ARCExpect supports the scalar CWL types `boolean`, `int`, `long`, `float`,
`double`, and `string`. Numeric syntax is invariant across all three runtimes.
Package metadata, rather than arc-validate, remains the single source of truth
for package-specific options.

### Configured package inputs

Use the parent resolver's exact package identity and configuration digest to run
a preflighted selection:

```bash
arc-validate validate \
  --arc-directory . \
  --package configurable-validation \
  --package-version 1.2.7 \
  --validation-config .arc/validation_packages.yml \
  --validation-config-sha256 0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef
```

Config-driven mode requires the package, package version, and explicit config
path together. It is mutually exclusive with raw arguments after `--`. The
digest is optional for direct interactive use but mandatory for generated
DataHUB jobs.

The child verifies the exact bytes before using values, rechecks the source
selection's exact or rolling-version intent, and loads only the named version
from the installed cache. It performs no registry request and deliberately does
not re-resolve whether that version is still the latest. Configured values are
validated against the cached package's declarations, ordered by binding
position and ordinal ID, and passed as separate process arguments.

See [Resolve validation configuration](validation-configuration.md) for the
parent plan contract and full child-execution rules.

### The package command

```
USAGE: arc-validate package [--help] [<subcommand> [<options>]]

SUBCOMMANDS:

    install, i <options>  install validation packages
    uninstall, u <options>
                          uninstall validation packages
    list, l               list packages available from the configured registry

    Use 'arc-validate <subcommand> --help' for additional information.

OPTIONS:

    --help, -h            display this list of options.
```

#### The package install subcommand

```
USAGE: arc-validate package install [--help] [--version <version>]
                                    <package name>

PACKAGE:

    <package name>        name of the validation package to install

OPTIONS:

    --version, -v <version>
                          Version to install. Defaults to the latest version.
    --help, -h            display this list of options.
```

#### The package uninstall subcommand

```
USAGE: arc-validate package uninstall [--help]
                                      [--packageversion <package version>]
                                      <package name>

PACKAGE:

    <package name>        name of the validation package to uninstall

OPTIONS:

    --packageversion, -pv <package version>
                          Version to uninstall. Without this option, all
                          installed versions of the package are removed.
    --help, -h            display this list of options.
```

#### The package list subcommand

```
USAGE: arc-validate package list

Lists packages and versions from the configured AVPR registry together with
their local installation state.
```
