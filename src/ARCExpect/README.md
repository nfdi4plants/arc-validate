# ARCExpect

ARCExpect provides portable ARC validation package authoring, execution, result
contracts, and output writers for .NET, JavaScript, and Python from one F#
source tree.

The source tree follows the DataHubClient/ARCtrl layout:

- `ARCExpect.fsproj` builds the full .NET library.
- `ARCExpect.Javascript.fsproj` transpiles the portable API to JavaScript.
- `ARCExpect.Python.fsproj` transpiles the portable API to Python.

The three project files compile the same ordered sources under `Common/`.
`DotNet/`, `Python/`, and `Javascript/` contain target runtime adapters
and native package facades. All targets expose the flat
`ARCExpect` namespace; the project-file suffixes are build boundaries, not
public namespaces. Distribution names are `ARCExpect` on NuGet,
`@nfdi4plants/arcexpect` for JavaScript, and `arcexpect` for Python.

`PackARCExpect` creates all three artifacts under `artifacts/packages`.
`TestPortableARCExpect` runs the shared contract suite on each runtime and then
installs each freshly packed artifact in an isolated consumer.

## Testing against unpublished AVPR packages

There is no hidden source-level link from ARCExpect or the CLI to the sibling
AVPR repository. The projects deliberately retain package references so local
tests exercise the same NuGet/npm/PyPI boundaries that released consumers use.
The mechanism has three separate parts:

1. The repository-root `Directory.Build.props` pins the exact AVPR client,
   interop, model, and codecs versions. Bracketed NuGet versions such as
   `[0.1.0-preview.4]` mean exactly that version, with no roll-forward.
2. `src/ARCExpect/Directory.Build.props` explicitly imports the root property
   file. MSBuild imports only the nearest `Directory.Build.props`, so this
   explicit import is required alongside ARCExpect's target-specific `obj/`
   folder settings.
3. The build helpers choose where those exact artifacts come from. They first
   use `AVPR_NATIVE_PACKAGE_DIR`, otherwise they detect
   `../arc-validate-package-registry/artifacts/packages`, and otherwise fall
   back to the public registries. For NuGet they generate an ignored temporary
   `NuGet.Config`; packed JavaScript and Python consumer tests install the
   matching `.tgz` and `.whl` files from the same directory directly.

The local artifact directory therefore needs the exact files named by
`Directory.Build.props`. For Step 3 that includes the Model and Codecs NuGet,
npm, and wheel artifacts plus the AVPRClient and AVPRClient.Interop NuGet
packages. Produce them in the AVPR checkout before testing this repository.

With the standard sibling layout, the named package and portable-test targets
discover the directory automatically:

```powershell
cd ..\arc-validate-package-registry
.\build.cmd PackPortablePackages
dotnet pack src\AVPRClient\AVPRClient.csproj -c Release -o artifacts\packages
dotnet pack src\AVPRClient.Interop\AVPRClient.Interop.csproj -c Release -o artifacts\packages

cd ..\arc-validate
.\build.cmd PackARCExpect
.\build.cmd TestPortableARCExpect
```

Run the two `dotnet pack` commands after `PackPortablePackages`: that named
AVPR target deliberately cleans `artifacts/packages` before writing the Model
and Codecs artifacts. The subsequent commands add the generated client and
interop NuGet packages without cleaning the native artifacts that the
JavaScript and Python consumer checks need.

For a non-sibling checkout, point the build at the artifact directory:

```powershell
$env:AVPR_NATIVE_PACKAGE_DIR = 'D:\work\avpr\artifacts\packages'
.\build.cmd TestPortableARCExpect
```

A plain IDE or `dotnet restore` invocation does not run the FAKE build helpers.
Until the selected previews are published, add the same directory explicitly:

```powershell
dotnet restore arc-validate.slnx `
  -p:RestoreAdditionalProjectSources='D:\work\avpr\artifacts\packages'
```

Once the exact versions are published, no local source or environment variable
is required. The same project files restore them from the public registries.
The `UseExplicitPackageVersions` target in `arc-validate.fsproj` is a separate
piece of pack-time plumbing: it makes a packed CLI advertise the exact
ARCExpect version from its in-repository `ProjectReference`; it does not locate
AVPR packages.

The npm artifact depends on `@nfdi4plants/validationpackage-model` and
`@nfdi4plants/validationpackage-codecs`; the Python artifact depends on the
corresponding unscoped PyPI distributions. Neither bundles Fable-generated
copies. The unpublished-package workflow above supplies those native
dependencies to packed-consumer tests.

The common API contains Pyxpecto package authoring and execution,
framework-neutral case and run results, validation summaries, Thoth.Json
codecs, JUnit XML generation, dependency-free SVG badges, and internal output
orchestration. The .NET target additionally contains filesystem
adapters, ARC specification validation, and the legacy
ARCTokenization/ControlledVocabulary validation helpers. The Python package
provides its native filesystem-writing pipeline in the package facade.

`ValidationSummary`, `Execute.Validation`, and the output writers accept
optional `SourceBranch` and `SourceCommitHash` values. Supplied values are
encoded as top-level JSON fields, JUnit `<properties>`, and an SVG `<metadata>`
element that is never rendered. Missing values are omitted, so existing output
remains unchanged. The .NET and Python filesystem pipelines consume the corresponding
`--source-branch` and `--source-commit-hash` process arguments forwarded by
arc-validate; portable code never reads process state or CI environment
variables.

arc-validate forwards four standard package arguments: `-i`, `-o`,
`--source-branch`, and `--source-commit-hash`. Package-defined arguments follow
the CLI's first `--` boundary. `PackageArguments.fromCommandLine(metadata)`
reads the target's native argument array and validates all values against the
metadata's CWL `Inputs`; `PackageArguments.parse(metadata, arguments)` provides
the same pure API for explicit arrays and browser JavaScript callers.

```fsharp
let metadata = Setup.Metadata(PACKAGE_METADATA)
let arguments = PackageArguments.fromCommandLine(metadata)

let arcDirectory = arguments.ArcDirectory
let outputDirectory = arguments.OutputDirectory
let testMode = arguments.TryGetBoolean("test")
let echo = arguments.TryGetString("echo")
```

ARCExpect selects F# frontmatter in the .NET build and Python frontmatter in
the Python build. Package authors never import or pass a language selector.
JavaScript metadata setup remains unavailable until AVPR defines a JavaScript
validation-package frontmatter format.

Typed `GetBoolean`/`TryGetBoolean`, `GetInt`/`TryGetInt`, `GetLong`, `GetFloat`,
`GetDouble`, and `GetString` pairs cover the supported CWL scalar types. A
`TryGet` result is absent only when a nullable input was not supplied; it does
not hide type or metadata errors. Unknown, duplicate, missing required, or
malformed values fail before package validation starts.

The equivalent native entry points are
`PackageArguments.fromCommandLine(metadata)` in JavaScript and
`PackageArguments.from_command_line(metadata)` in Python. Instance properties
and typed getter names retain their documented PascalCase API in both emitted
packages.

JavaScript and Python intentionally do not expose compatibility stubs for the
.NET-only CV APIs. Portable validation packages and the shared top-level
`Execute` facade use Fable.Pyxpecto test cases. Expecto is not part of the
production ARCExpect package surface.

The shared `Setup.ValidationPackage` accepts Pyxpecto test-case arrays and
`Execute.Validation` returns an F# `Async<ValidationSummary>`. The npm entry point
exposes the same operation as a Promise; Python consumers use
`await Execute.validation(...)`. Both native packages re-export Pyxpecto case
constructors such as `testCase`/`ptestCase` and `test_case`/`ptest_case`.

## Package authoring

Validation packages use `Fable.Pyxpecto` cases and the shared API. Published
packages pinned to historical ARCExpect versions remain unchanged; new package
versions target the Pyxpecto surface:

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

let arguments = PackageArguments.fromCommandLine(metadata)
validationPackage |> Execute.ValidationPipeline(arguments)
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
summary = await Execute.validation_pipeline(validation_package)
```

The .NET and Python pipelines write the standard summary JSON, JUnit XML, and
badge SVG under `.arc-validate-results/<name>@<version>/`. Internally the shared
pipeline performs result combination and encoding so package
authors do not have to orchestrate individual writers for the standard flow.
The individual `Execute.SummaryCreation`, `Execute.JUnitReportCreation`, and
`Execute.BadgeCreation` methods remain public on .NET, with snake-case
equivalents in Python. Pure summary, JUnit, and badge encoders remain public on
all targets. JavaScript validation-package execution and filesystem conventions
remain deliberately “Coming soon”.
