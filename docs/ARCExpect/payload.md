# Add a payload to the validation summary

ARCExpect can attach arbitrary JSON data to `validation_summary.json` under
the optional top-level `Payload` field. Use it for calculated metrics or
structured findings that do not belong in the pass/fail test counts.

The payload is created by package code after collecting the relevant data. Use
an `IDictionary<string, obj>` with JSON-compatible nested values in F#/.NET. In
Python, use ordinary dictionaries, lists, strings, numbers, booleans, and
`None`. ARCExpect converts each target's native values to its shared JSON model
at the package boundary. Pass the result to the validation pipeline as
`Payload` in F# or `payload` in Python.

The complete package below counts files in the supplied ARC and records that
calculation as `Payload.Metrics.FilesChecked`. `RunDocsSamples` creates a
deterministic two-file ARC, executes both implementations, and asserts that
both summaries contain the value `2`.

=== "F# / .NET"

    ```fsharp
    --8<-- "docs/samples/payload/sample.fsx"
    ```

=== "Python"

    ```python
    --8<-- "docs/samples/payload/sample.py"
    ```

=== "JavaScript"

    **Coming soon.** JavaScript validation-package execution and frontmatter
    conventions are not defined yet.

## Generated validation outputs

The payload appears in the summary JSON. JUnit and badge output continue to
represent test results and source provenance; they do not duplicate arbitrary
payload data.

### Badge

![Payload validation package badge](../samples/payload/badge.svg)

### Validation summary

```json
--8<-- "docs/samples/payload/validation_summary.json"
```

### JUnit report

```xml
--8<-- "docs/samples/payload/validation_report.xml"
```
