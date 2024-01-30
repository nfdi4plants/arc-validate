# arc-validate

`arc-validate` is a CLI tool for validating ARCs and managing validation packages.

## Installation

## Command Line Usage

```
USAGE: arc-validate [--help] [--verbose] [--token <string>] [<subcommand> [<options>]]

SUBCOMMANDS:

    validate, v <options> command for performing arc validation
    package, p <options>  subcommands for validation packages

    Use 'arc-validate <subcommand> --help' for additional information.

OPTIONS:

    --verbose, -v         Use verbose error messages (with full error stack).
    --token, -t <string>  The token to use for authentication with github.
    --help, -h            display this list of options.
```

### The validate command

```
USAGE: arc-validate validate [--help] [--arc-directory <path>] [--out-directory <path>] [--package <package name>]

OPTIONS:

    --arc-directory, -i <path>
                          Optional. Specify a directory that contains the arc to convert. Default: content of the ARC_PATH
                          environment variable. If ARC_PATH is not set: current directory.
    --out-directory, -o <path>
                          Optional. Specify a output directory for the test results file (arc-validate-results.xml).
                          Default: file gets written to the arc root folder.
    --package, -p <package name>
                          Optional. Specify a validation package to use on top of the default validation for invenio
                          export. Default: no package is used, only structural validation for invenio export.
    --help, -h            display this list of options.
```

### The package command

```
USAGE: arc-validate package [--help] [<subcommand> [<options>]]

SUBCOMMANDS:

    install, i <options>  install valiation packages
    uninstall, u <options>
                          uninstall valiation packages
    list, l <options>     list packages from available soures
    update-index, c       update the locally chached package index

    Use 'arc-validate <subcommand> --help' for additional information.

OPTIONS:

    --help, -h            display this list of options.
```

#### The package install subcommand

```
USAGE: arc-validate package install [--help] <package name>

PACKAGE:

    <package name>        name of the validation package to install

OPTIONS:

    --help, -h            display this list of options.
```

#### The package uninstall subcommand

```
USAGE: arc-validate package uninstall [--help] <package name>

PACKAGE:

    <package name>        name of the validation package to uninstall

OPTIONS:

    --help, -h            display this list of options.
```

#### The package list subcommand

```
USAGE: arc-validate package list [--help] [--installed] [--indexed]

OPTIONS:

    --installed, -i       list installed packages from the package cache
    --indexed, -c         list indexed packages from the cached package index
    --help, -h            display this list of options.
```