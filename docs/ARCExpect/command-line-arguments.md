# Add typed command-line arguments

ARCExpect owns command-line parsing for validation packages. Package authors
declare CWL inputs in the YAML frontmatter's `Inputs` collection, parse that
frontmatter with `Setup.Metadata`, call
`PackageArguments.fromCommandLine`, and use typed getters. No target-specific
argument parser is necessary.

See the [AVPR metadata documentation](https://github.com/nfdi4plants/arc-validate-package-registry/blob/dev/docs/packages/metadata.md)
for the authoritative metadata schema and supported CWL input fields. This
guide covers consuming those declared inputs inside ARCExpect.

## Complete package

This example declares:

- a required boolean `--strict` flag;
- a required integer `--minimum-files VALUE`; and
- an optional string `--label VALUE`.

It also reads the four standard package arguments supplied by arc-validate:
the ARC directory, output directory, source branch, and source commit hash.

=== "F# / .NET"

    ```fsharp
    --8<-- "docs/samples/command-line-arguments/sample.fsx"
    ```

=== "Python"

    ```python
    --8<-- "docs/samples/command-line-arguments/sample.py"
    ```

=== "JavaScript"

    **Coming soon.** JavaScript validation-package execution and frontmatter
    conventions are not defined yet.

## Generated validation outputs

The source branch and commit supplied above are encoded into all three standard
outputs. `RunDocsSamples` regenerates these files from the F#/.NET package and
verifies the corresponding Python output during every sample verification pass.

### Badge

![Configurable validation package badge](../samples/command-line-arguments/badge.svg)

### Validation summary

```json
--8<-- "docs/samples/command-line-arguments/validation_summary.json"
```

### JUnit report

```xml
--8<-- "docs/samples/command-line-arguments/validation_report.xml"
```

## Invoke it through arc-validate

The first argument exactly equal to `--` separates arc-validate options from
package options:

```sh
arc-validate validate \
  --package configurable-validation \
  --arc-directory ./arc \
  --out-directory ./validation-results \
  --source-branch feature/validation \
  --source-commit-hash 0123456789abcdef \
  -- \
  --strict \
  --minimum-files 2 \
  --label "release candidate"
```

`--source-branch` is not a boundary: only the standalone `--` token has that
meaning. arc-validate removes the boundary and passes the standard and custom
tokens to the package as one argument list without invoking a shell.

## CWL binding behavior

- A boolean prefix is a flag. Present means `true`; an absent required boolean
  means `false`; an absent nullable `boolean?` has no value.
- `separate: true` consumes the next argument as the value.
- `separate: false` expects a value joined directly to its prefix.
- Inputs without a prefix are positional and ordered by `position`.
- Supported scalar types are `boolean`, `int`, `long`, `float`, `double`, and
  `string`; numeric syntax is invariant across runtimes.
- Unknown, duplicate, malformed, missing required, or reserved-name inputs fail
  before validation begins.

The returned strings are data, not shell commands. If package code later starts
another process, it must continue passing values through an argument-list API
instead of building a shell command string.
