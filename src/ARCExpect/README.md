# ARCExpect

ARCExpect provides portable ARC validation result contracts and output writers
for .NET, JavaScript, and Python from one F# source tree.

The source tree follows the DataHubClient/ARCtrl layout:

- `ARCExpect.fsproj` builds the full .NET library.
- `ARCExpect.Javascript.fsproj` transpiles the portable API to JavaScript.
- `ARCExpect.Python.fsproj` transpiles the portable API to Python.

The three project files compile the same ordered root sources. Only the
.NET project additionally compiles `DotNet/`. All targets expose the flat
`ARCExpect` namespace; the project-file suffixes are build boundaries, not
public namespaces. Distribution names are `ARCExpect` on NuGet and
`arcexpect` for JavaScript and Python.

`PackARCExpect` creates all three artifacts under `artifacts/packages`.
`TestPortableARCExpect` runs the shared contract suite on each runtime and then
installs each freshly packed artifact in an isolated consumer.

The portable API contains framework-neutral case and run results, validation
summaries, Thoth.Json codecs, JUnit XML generation, and dependency-free SVG
badges. The .NET target additionally contains the current Expecto execution and
filesystem adapters, ARC specification validation, and the legacy
ARCTokenization/ControlledVocabulary validation helpers.

JavaScript and Python intentionally do not expose compatibility stubs for the
.NET-only CV APIs. The final portable validation-case API will use
Fable.Pyxpecto after its structured result surface is available.
