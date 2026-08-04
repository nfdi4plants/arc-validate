# Create a simple validation package

ARCExpect uses [Fable.Pyxpecto](https://github.com/Freymaurer/Fable.Pyxpecto)
test cases and converts their outcomes into a framework-neutral
`ValidationSummary`. The same package structure works on .NET, Python, and
JavaScript.

## Install ARCExpect

=== "F# / .NET"

    ```sh
    dotnet add package ARCExpect
    ```

    F# scripts can use `#r "nuget: ARCExpect"`, as in the complete sample below.

=== "Python"

    ```sh
    pip install arcexpect
    ```

=== "JavaScript"

    **Coming soon.** JavaScript validation-package execution and frontmatter
    conventions are not defined yet.

## Define and execute a package

Every package begins with YAML frontmatter bound to `PACKAGE_METADATA`. The
package code passes that exact value to `Setup.Metadata`; users should not
reconstruct registry metadata programmatically. Critical and non-critical cases
are executed independently and retained as separate result groups in the
summary.

The frontmatter must be the first construct in the file. Put NuGet directives
and imports only after its closing delimiter.

The authoritative frontmatter schema, supported fields, and submission rules
live in the [AVPR metadata documentation](https://github.com/nfdi4plants/arc-validate-package-registry/blob/dev/docs/packages/metadata.md).
This guide focuses on using ARCExpect after the package metadata has been
declared.

=== "F# / .NET"

    ```fsharp
    --8<-- "docs/samples/simple-validation-package/sample.fsx"
    ```

=== "Python"

    ```python
    --8<-- "docs/samples/simple-validation-package/sample.py"
    ```

=== "JavaScript"

    **Coming soon.** JavaScript validation-package execution and frontmatter
    conventions are not defined yet.

## Generated validation outputs

The package writes the standard validation summary, JUnit report, and badge to
the output directory supplied by arc-validate. `RunDocsSamples` regenerates the
files below from the F#/.NET package and verifies that the Python package emits
the same portable summary and badge plus a valid JUnit report.

### Badge

![Simple validation package badge](../samples/simple-validation-package/badge.svg)

### Validation summary

```json
--8<-- "docs/samples/simple-validation-package/validation_summary.json"
```

### JUnit report

```xml
--8<-- "docs/samples/simple-validation-package/validation_report.xml"
```

`Execute.Validation` returns asynchronously on every target: `Async` in F#,
an awaitable in Python, and a `Promise` in JavaScript. A failed assertion is
captured in the summary; an unexpected exception is recorded as an error.

The portable `ValidationSummary`, `JUnit.Writer`, and `Badge.Writer` APIs can
then encode the standard JSON, JUnit XML, and SVG outputs. The .NET compatibility
pipeline additionally provides the established filesystem-writing behavior.

## Next step

For configurable packages, declare CWL `Inputs` in the metadata and read the
validated values through [ARCExpect command-line arguments](command-line-arguments.md).
