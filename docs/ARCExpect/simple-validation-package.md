# Create a simple validation package

ARCExpect uses [Fable.Pyxpecto](https://github.com/Freymaurer/Fable.Pyxpecto)
test cases and converts their outcomes into a framework-neutral
`ValidationSummary`. The same package structure works on .NET, Python, and
JavaScript.

This first package has no package-defined arguments and creates no custom
payload. It only consumes the standard ARC and output paths supplied by the
validation-package runtime. The next guide adds a calculated JSON payload;
the following guide adds typed package arguments.

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

ARCExpect selects the frontmatter language from the installed target: the
.NET package parses F# frontmatter and the Python package parses the bound
Python frontmatter value. Package code does not import or pass a language enum.

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

`Execute.Validation` remains available when a caller only needs the portable
summary. The higher-level `Execute.ValidationPipeline` API used by the samples
executes the package, combines its critical and non-critical results, creates
all three output formats, and writes them below
`.arc-validate-results/<name>@<version>/`. Package authors do not need to call
the individual summary, JUnit, or badge writers for the standard workflow. The
pipeline is synchronous in F#/.NET and awaitable in Python. JavaScript package
execution and its output boundary are intentionally still undesigned.

The individual output operations remain public for custom workflows. On .NET,
pipe a `ValidationSummary` into `Execute.SummaryCreation`,
`Execute.JUnitReportCreation`, or `Execute.BadgeCreation`; Python exposes the
equivalent `Execute.summary_creation`, `Execute.junit_report_creation`, and
`Execute.badge_creation` methods. The pure `ValidationSummary`, `JUnit.Writer`,
and `Badge.Writer` encoders are available when callers want content without
filesystem writes.

## Next step

Continue with [adding a calculated result payload](payload.md), then declare
CWL `Inputs` for configurable packages in
[ARCExpect command-line arguments](command-line-arguments.md).
