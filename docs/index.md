# arc-validate and ARCExpect

This repository provides the tools for creating and running reusable ARC
validation packages:

- **ARCExpect** is the polyglot validation library. One F# source tree ships as
  NuGet, Python, and npm packages with the same result and output contracts.
- **arc-validate** is the command-line application that installs and executes
  validation packages for an ARC.

Start with [a simple ARCExpect validation package](ARCExpect/simple-validation-package.md),
then add [typed command-line inputs](ARCExpect/command-line-arguments.md) when a
package needs configuration.

!!! info "The language tabs contain executable programs"
    Every available F# and Python validation-package example is included from
    `docs/samples/`. The `RunDocsSamples` build target packs ARCExpect, installs
    each artifact like an end user, and executes the samples. JavaScript tabs
    remain explicitly marked “Coming soon” until a JavaScript package format is
    designed.

## Package ecosystem

| Runtime | ARCExpect package | Shared metadata packages |
|---------|-------------------|--------------------------|
| .NET / F# | `ARCExpect` | `ValidationPackage.Model`, `ValidationPackage.Codecs` |
| Python | `arcexpect` | `validationpackage-model`, `validationpackage-codecs` |
| JavaScript | `@nfdi4plants/arcexpect` | `@nfdi4plants/validationpackage-model`, `@nfdi4plants/validationpackage-codecs` |

The metadata packages are dependencies of ARCExpect and are installed by the
normal package manager flow.
