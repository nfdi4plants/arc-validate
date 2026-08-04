# ARCExpect

ARCExpect provides portable ARC validation package authoring, execution, result
contracts, and output writers for .NET, JavaScript, and Python from one F#
source tree.

The source tree follows the DataHubClient/ARCtrl layout:

- `ARCExpect.fsproj` builds the full .NET library.
- `ARCExpect.Javascript.fsproj` transpiles the portable API to JavaScript.
- `ARCExpect.Python.fsproj` transpiles the portable API to Python.

The three project files compile the same ordered root sources. Only the
.NET project additionally compiles `DotNet/`. All targets expose the flat
`ARCExpect` namespace; the project-file suffixes are build boundaries, not
public namespaces. Distribution names are `ARCExpect` on NuGet,
`@nfdi4plants/arcexpect` for JavaScript, and `arcexpect` for Python.

`PackARCExpect` creates all three artifacts under `artifacts/packages`.
`TestPortableARCExpect` runs the shared contract suite on each runtime and then
installs each freshly packed artifact in an isolated consumer.

The npm artifact depends on `@nfdi4plants/validationpackage-model` and
`@nfdi4plants/validationpackage-codecs`; the Python artifact depends on the
corresponding unscoped PyPI distributions. Neither bundles Fable-generated
copies. Before those dependencies are published, set `AVPR_NATIVE_PACKAGE_DIR`
to the AVPR `artifacts/packages` directory when running the packed-consumer
target.

The portable API contains framework-neutral case and run results, validation
summaries, Thoth.Json codecs, JUnit XML generation, and dependency-free SVG
badges. The .NET target additionally contains the current Expecto execution and
filesystem adapters, ARC specification validation, and the legacy
ARCTokenization/ControlledVocabulary validation helpers.

`ValidationSummary`, `Execute.Validation`, and the output writers accept
optional `SourceBranch` and `SourceCommitHash` values. Supplied values are
encoded as top-level JSON fields, JUnit `<properties>`, and an SVG `<metadata>`
element that is never rendered. Missing values are omitted, so existing output
remains unchanged. The .NET filesystem pipeline also consumes the corresponding
`--source-branch` and `--source-commit-hash` process arguments forwarded by
arc-validate; portable code never reads process state or CI environment
variables.

arc-validate currently forwards four standard package arguments: `-i`, `-o`,
`--source-branch`, and `--source-commit-hash`. ARCExpect does not yet expose a
cross-target API that reads all four, and CWL `Inputs` do not yet create or
parse arbitrary CLI options. That argument API is intentionally deferred until
the current portable package batch has been reviewed.

JavaScript and Python intentionally do not expose compatibility stubs for the
.NET-only CV APIs. Portable validation packages and the shared top-level
`Execute` facade use Fable.Pyxpecto test cases; the existing Expecto runner and
filesystem writes remain .NET-only compatibility APIs.

The shared `Setup.ValidationPackage` accepts Pyxpecto test-case arrays and
`Execute.Validation` returns an F# `Async<ValidationSummary>`. The npm entry point
exposes the same operation as a Promise; Python consumers use
`await Execute.validation(...)`. Both native packages re-export Pyxpecto case
constructors such as `testCase`/`ptestCase` and `test_case`/`ptest_case`.

## Migrating package authoring from Expecto

Existing .NET validation packages can continue using the Expecto overloads
during the compatibility window. New portable packages should use
`Fable.Pyxpecto` cases and the shared API:

```fsharp
open ARCExpect
open Fable.Pyxpecto

let validationPackage =
    Setup.ValidationPackage(
        metadata,
        CriticalValidationCases = [|
            testCase "required metadata" <| fun () -> ()
        |]
    )

let summary = Execute.Validation(validationPackage) |> Async.RunSynchronously
```

The equivalent JavaScript API returns a Promise:

```javascript
import { Execute, Setup, testCase } from "@nfdi4plants/arcexpect";

const validationPackage = Setup.ValidationPackage(metadata, [
  testCase("required metadata", () => {})
]);
const summary = await Execute.Validation(validationPackage);
```

The equivalent Python API is awaitable:

```python
from arcexpect import Execute, Setup, test_case

validation_package = Setup.ValidationPackage(
    metadata,
    [test_case("required metadata", lambda: None)],
)
summary = await Execute.validation(validation_package)
```

Move filesystem output orchestration to the caller when targeting JavaScript
or Python. The legacy Expecto runner and filesystem-writing pipeline remain
available only from the .NET package and are not portable APIs.
