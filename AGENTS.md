# AGENTS.md

## Repository purpose

This repository contains the `arc-validate` command-line application and the
F# libraries used to author, execute, and report ARC validation packages. The
CLI is used in DataHUB validation pipelines and supports validation packages
retrieved from the ARC validation package registry (AVPR).

The CLI remains .NET-oriented, while ARCExpect is a polyglot library compiled
for .NET and transpiled to JavaScript and Python.
Treat portability and the emitted JavaScript/Python API as public contract
concerns whenever a project or source boundary is designated portable.

The cross-repository roadmap is maintained in:

`../arc-validate-package-registry/plans/portable-validation-package-boundaries/roadmap.md`

The AVPR repository owns the shared validation-package model, codecs, registry
service, generated HTTP client, and client/model interop. Do not duplicate
those responsibilities here.

Keep the roadmap issues separate:

- arc-validate #242: portable ARCExpect result and output contracts.
- arc-validate #243: absorb ARCValidationPackages infrastructure into the CLI.
- arc-validate #244: build ARCExpect through three parallel .NET, JavaScript, and
  Python project files, with CV helpers retained only on .NET.
- arc-validate #245: provide shared Pyxpecto package authoring and top-level
  execution, using the focused result adapter until Pyxpecto exposes structured results.

## Repository map

- `src/ARCExpect/`: one ARCExpect source tree with three parallel project files:
  `ARCExpect.fsproj` for .NET, `ARCExpect.Javascript.fsproj` for Fable
  JavaScript/TypeScript, and `ARCExpect.Python.fsproj` for Fable Python.
  `Common/` is compiled by all three. `DotNet/`, `Python/`, and
  `Javascript/` contain target runtime adapters and native package facades;
  target folders are implementation boundaries and never public namespaces.
- `src/arc-validate/PackageManagement/`: internal registry access,
  configuration, cache management, installation, and uninstallation.
- `src/arc-validate/PackageRunner/`: internal F# and Python script execution.
- `src/arc-validate/`: CLI arguments, commands, orchestration, presentation,
  package management, and package execution.
- `tests/ARCExpect.Tests/`: .NET filesystem pipeline, specification, and CV
  tests. Expecto may be used as this test project's runner, but is not a
  production ARCExpect dependency.
- `tests/ARCExpect.Contract.Tests/`: shared Fable.Pyxpecto contract suite for
  portable ARCExpect APIs. The JavaScript and Python sibling projects compile
  the same test sources against their corresponding ARCExpect project.
- `tests/arc-validate.Tests/PackageManagement/`: registry, cache,
  configuration, and script-execution tests.
- `tests/arc-validate.Tests/`: CLI and output-contract tests.
- `tests/Common/`: shared .NET test helpers.
- `build/`: BlackFox/FAKE build project containing build, test, pack,
  documentation, and release targets.
- `.github/workflows/`: cross-platform build/test, documentation, and container
  publication workflows.
- `docs/`: MkDocs guide pages, fsdocs API-reference input, and executable
  validation-package samples under `docs/samples/<topic>/sample.{fsx,py}`.
- `mkdocs.yml` and `overrides/`: Material guide configuration and theme
  override; generated guide/API output belongs under ignored `site/`.

## Toolchain and common commands

The SDK is pinned in `global.json` (currently .NET 10). Python validation
package execution uses `uv`; do not install Python dependencies ad hoc into a
system interpreter.

```shell
# Default solution build
.\build.cmd

# Full build/test orchestration
.\build.cmd RunTests

# Direct solution build
dotnet build arc-validate.slnx -m:1

# Focused test projects
dotnet test tests/ARCExpect.Tests/ARCExpect.Tests.fsproj
dotnet run --project tests/ARCExpect.Contract.Tests/ARCExpect.Contract.Tests.fsproj
dotnet test tests/arc-validate.Tests/PackageManagement/arc-validate.PackageManagement.Tests.fsproj
dotnet test tests/arc-validate.Tests/arc-validate.Tests.fsproj

# Shared ARCExpect contracts on .NET, JavaScript, and Python
./build.cmd TestPortableARCExpect

# Build ARCExpect NuGet, npm, and wheel artifacts under artifacts/packages
.\build.cmd PackARCExpect

# Documentation
.\build.cmd BuildDocs
.\build.cmd RunDocsSamples
.\build.cmd WatchDocs
.\build.cmd WatchApiDocs

# Create local NuGet artifacts; these targets are interactive
.\build.cmd Pack
.\build.cmd PackPrerelease
```

Use `./build.sh` instead of `.\build.cmd` on Linux or macOS. Prefer focused
builds/tests while developing, then run the affected solution-level target
before handing off.

`RunTests` is not hermetic today: it can clone the `invenio-test-arc` fixture,
publishes the CLI locally before testing, requires `uv`, and includes legacy
package-management contract tests that call the AVPR development service. Some
legacy cache tests write under the platform application-data directory. Inspect
these side effects before running the full target in a restricted or shared
environment. Do not add new tests that depend on live services.

## Build-project conventions

- Keep build and CI command details in the `build/` project. Workflows should
  install platform toolchains and invoke a named build target instead of
  duplicating orchestration in YAML.
- Register every buildable, testable, or published project in
  `build/ProjectInfo.fs` as appropriate. Published projects need their own
  release notes, version ownership, pack target, and focused tests.
- ARCExpect has one source tree and one API identity, implemented by three
  parallel project files following the DataHubClient/ARCtrl pattern. Keep the
  portable `<Compile>` lists duplicated, ordered, and synchronized in all
  three projects; do not factor them into an imported props file.
- `Directory.Build.props` owns `ARCExpectPackageVersion` and the exact AVPR
  package pins. The build verifies the ARCExpect value against the latest
  release-notes entry and derives native manifest versions from these
  properties; do not introduce another hand-maintained version constant.
- The .NET NuGet package is `ARCExpect`; JavaScript and Python distributions
  use the package name `arcexpect`. Do not expose `Core`,
  `Portable`, `.NET`, `.Javascript`, or `.Python` in public namespaces.
- Keep targets composable: separate restore/build/test/transpile/pack from
  external publication. A target that verifies or packs an artifact must not
  publish it as a side effect.
- Add named portable targets that run the same contract suite on .NET,
  JavaScript, and Python and verify packed consumers. Keep the Fable compiler
  and Python `fable-library` pins coordinated.
- Put generated/transpiled output under ignored `artifacts/`; never commit it.
  New or modernized pack targets should emit to `artifacts/packages/`, matching
  AVPR. The current `pkg/` output is legacy until those targets are migrated;
  do not create additional artifact layouts.
- ARCExpect packaging follows DataHubClient: `ARCExpect.fsproj` produces the
  NuGet package, while the JavaScript and Python projects are transpiled into
  separate `arcexpect` npm and wheel artifacts. Do not use the NuGet package as
  the Fable source-distribution mechanism.
- Keep F# source order explicit in every `.fsproj`. Adding or moving a source
  file requires updating project order deliberately.
- Do not invoke `Release`, `PreRelease`, `ReleaseNoDocs`,
  `PreReleaseNoDocs`, tag-push, NuGet-push, documentation-push, or container
  publication paths unless the user explicitly requests the external action
  and provides the required authorization.

## Code and test conventions

- Follow the style already present in the touched project: F# modules,
  pipeline-oriented transformations, explicit domain functions, and small
  compatibility adapters.
- Preserve public behavior and output schemas unless the task explicitly
  authorizes a breaking change.
- Existing .NET test projects may retain their Expecto/YoloDev test harness.
  Production ARCExpect validation packages use Fable.Pyxpecto only. Give tests
  behavior-oriented names consistent with neighboring tests.
- Use small in-repository fixtures and injected/local HTTP handlers for new
  package-management tests. Do not extend the current live-AVPR test pattern.
- Keep filesystem, HTTP, process execution, serialization, and framework
  adapters at explicit boundaries so pure logic can be tested without external
  state.
- Preserve unrelated working-tree changes. Avoid broad formatting, line-ending,
  generated-file, or dependency churn unrelated to the task.
- Do not edit `bin/`, `obj/`, `.vs/`, `.fake/`, `publish/`, or generated
  `artifacts/` output.
- Validation package scripts may access the network, traverse input data, or do
  substantial computation. Read a script before executing it locally.

## Portable Fable code style

Portable code is compiled from the same ordered F# files by the .NET,
JavaScript, and Python ARCExpect projects. Treat its native API shape and
cross-target behavior as part of the public contract.

- Keep portable projects free of filesystem and directory APIs, process
  execution, environment access, HTTP, FAKE, Expecto, System.Text.Json,
  FSharp.SystemTextJson, generated clients, and other .NET-only infrastructure.
  Those concerns belong in .NET compatibility or application boundaries.
- Keep serialization out of domain/result types. Portable YAML/frontmatter and
  domain JSON behavior belongs to `ValidationPackage.Codecs`; output writers
  should generate content from portable contracts without performing file I/O.
- Prefer public classes over records when values are intended for direct use
  from JavaScript or Python. Mark every public portable class with
  `[<AttachMembers>]` so instance and static members stay attached to the
  emitted class.
- Implement settable public properties with explicitly named mutable backing
  fields such as `_name`. Do not use `member val ... with get, set` on portable
  public classes: Fable emits compiler-generated fields such as `Name@`, which
  leak an awkward native API.
- Do not shadow constructor parameters with backing fields.
- Prefer static members on public classes when behavior belongs to a domain
  type. Private implementation modules are fine; avoid public modules that
  transpile into detached functions for class-owned behavior.
- Avoid reflection and target-specific standard-library APIs. When a BCL or
  FSharp.Core API may behave differently across targets, cover it in the shared
  cross-target suite.
- Make null, option, collection, and enum/union representations intentional at
  JavaScript and Python boundaries. Do not assume .NET serialization or
  reflection behavior survives transpilation.
- Keep pure transformations deterministic. Inject time, environment, and other
  ambient state at the compatibility boundary.
- After changing a portable public type, transpile it and inspect generated
  JavaScript and Python under `artifacts/`. Check for detached functions,
  `@`-suffixed fields, mangled public names, unexpected wrapper shapes, and
  target-only failures.

## Portable tests and runtime setup

Portable contract tests use only `Fable.Pyxpecto` as their test framework.
They are regular executables, not VSTest projects.

- Run the .NET form with `dotnet run`, not `dotnet test`.
- Transpile the same suite with the pinned local Fable tool, then run the
  emitted entry point with Node and with the `uv`-managed Python interpreter.
- Keep the root `package.json` marked as `"type": "module"` when JavaScript
  output is introduced so Node treats Fable output as ESM.
- Declare Python runtime dependencies in the root `pyproject.toml`, commit
  `uv.lock`, and run Python output with `uv run`.
- `.venv` is platform-specific and must remain ignored. Recreate it after
  moving a workspace between operating systems; never reuse a foreign-platform
  environment.
- Treat Fable compiler and `fable-library` upgrades as coordinated changes:
  update the lockfile, run all three targets, inspect generated APIs, and run
  packed-consumer checks.

## Polyglot documentation

The documentation scaffold follows DataHubClient's verified-sample pattern.
MkDocs Material renders the guide and language tabs; fsdocs is restricted to
the generated F# API reference under `site/fsdocs/`.

- Never duplicate a substantial code sample directly in Markdown. Put one
  runnable validation package per supported format under
  `docs/samples/<topic>/sample.{fsx,py}` and include it with
  `pymdownx.snippets` in synchronized language tabs. Keep the JavaScript tab as
  “Coming soon” until JavaScript package execution and frontmatter conventions
  are deliberately designed; do not invent them in documentation.
- `RunDocsSamples` must pack ARCExpect, install the relevant NuGet/wheel
  artifacts like an end user, and execute every committed sample. Samples must
  be deterministic and offline except for package restoration. Each package
  writes `validation_summary.json`, `validation_report.xml`, and `badge.svg`.
  The runner regenerates the checked-in output under the sample topic from the
  F#/.NET execution, pretty-prints JSON and XML with stable line endings, and
  normalizes JUnit case durations to `0.000`. It then requires Python to emit
  the same summary, JUnit report, and badge byte-for-byte. Documentation pages
  display all three generated artifacts; do not hand-edit them.
- Keep F# sample references pinned to the exact current ARCExpect version in
  the committed `.fsx`; the runner injects only the local package source into
  its scratch copy. Python samples run unmodified against an isolated wheel
  install.
- Package samples begin with AVPR-compatible YAML frontmatter bound to
  `PACKAGE_METADATA` as the very first construct in the file. No imports,
  directives, comments, or executable code may precede it. Imports and NuGet
  references follow the complete frontmatter block, and package code parses the
  bound value with `Setup.Metadata`. Do not construct metadata programmatically
  in practical guides. Link to AVPR documentation for the authoritative schema
  instead of duplicating its field reference here.
- Documentation packages declare ARCExpect explicitly after frontmatter: an
  exact `#r "nuget: ARCExpect, ..."` in F# and a PEP 723 dependency in Python.
  Keep both pins synchronized with the ARCExpect release-notes version; the
  sample runner rejects stale pins and injects only the local artifact source.
- `BuildDocs` builds MkDocs into `site/` first and fsdocs into
  `site/fsdocs/` second because MkDocs cleans the shared output root.
- `WatchDocs` previews the guide. `WatchApiDocs` separately previews the F# API
  reference. Do not expect the fsdocs link to work in a guide-only preview.
- The root `pyproject.toml` owns the MkDocs dependency group and `uv.lock` must
  be updated whenever documentation tooling changes.
- Documentation CI runs samples and a strict site build on `dev`/`release`
  pushes and pull requests. Only a `release` push or manual dispatch deploys
  GitHub Pages.

## ARCExpect portability boundary

The roadmap separates framework-neutral result/output contracts from
target-specific filesystem behavior.

- Replace validation-package metadata/frontmatter dependencies with
  `ValidationPackage.Model` and `ValidationPackage.Codecs`; never reintroduce
  `AVPRIndex`.
- Portable ARCExpect owns framework-neutral case outcomes, per-case results,
  run summaries, and pure summary/JUnit/badge content generation.
- Keep `Thoth.Json.Core.Json` as the canonical payload stored in portable
  summaries. Native package boundaries convert JSON-compatible author values:
  `IDictionary<string, obj>` on .NET, dictionaries/lists/scalars on Python,
  and plain objects/arrays/scalars on JavaScript. All targets reject non-finite
  payload numbers.
- Keep owned output APIs under `ARCExpect.Badge` and `ARCExpect.JUnit`; do not
  reintroduce AnyBadge.NET or platform XML dependencies into ARCExpect.
- The internal output pipeline owns portable result combination and
  summary/JUnit/badge encoding. Do not expose that bundle as a public package
  authoring type; authors should use `Execute.ValidationPipeline` rather than
  combining `RunSummary` values for the standard workflow. Preserve the public
  individual `Execute.SummaryCreation`, `Execute.JUnitReportCreation`, and
  `Execute.BadgeCreation` operations for custom workflows, along with the pure
  content writers.
- Filesystem writes remain target-specific: .NET owns them under `DotNet/`,
  Python owns them in `Python/top_level_api.py`, and JavaScript package/file
  conventions remain intentionally undesigned.
- `Setup.Metadata(PACKAGE_METADATA)` chooses the frontmatter language from
  the compiled target. Do not expose an ARCExpect language selector or mirror
  the codec union. AVPR Codecs retains its own union because registry tooling
  must parse multiple source languages. JavaScript metadata setup remains
  explicitly unavailable until AVPR defines a JavaScript package/frontmatter
  format.
- Preserve the existing output layout and semantics:
  `.arc-validate-results/<name>@<version>/`, `validation_summary.json`,
  `validation_report.xml`, and `badge.svg`.
- Source provenance is explicit and optional. `arc-validate validate` accepts
  `--source-branch` and `--source-commit-hash`, forwards supplied values to
  package processes, and never infers them from CI environment variables.
  Omit absent values from summary JSON, JUnit properties, and SVG metadata.
- The CLI forwards `-i`, `-o`, `--source-branch`, and
  `--source-commit-hash`, followed by package-defined arguments after the first
  `--` boundary. Keep the left/right boundary strict and preserve right-side
  tokens as process argument-list elements; never construct a shell command.
- `PackageArguments` owns shared standard/CWL parsing and typed getters.
  Target-specific readers are limited to .NET `Environment`, Node `process.argv`,
  and Python `sys.argv`; browser JavaScript callers use the pure `parse` API.
- CWL input prefixes must not shadow standard arguments. Unknown, duplicate,
  malformed, and missing required values fail rather than being silently
  accepted. Keep invariant scalar behavior covered by all three contract suites.
- Prefer semantic/schema equivalence across targets over incidental whitespace
  or serializer formatting equality.
- ARCTokenization and ControlledVocabulary compatibility APIs belong only to
  the .NET project. JavaScript and Python intentionally expose no stubs for
  those APIs. ARCGraph/OboGraph and the direct OBO.NET, Graphoscope, and
  Cytoscape.NET dependencies were retired; do not reintroduce them.
- `Setup`, `ARCValidationPackage`, and the top-level `Execute` facade are
  required shared APIs on .NET, JavaScript, and Python; output writers alone
  are not a complete transpiled ARCExpect surface.
- Keep native top-level facade implementations explicit under
  `DotNet/TopLevelAPI.fs`, `Python/top_level_api.py`, and
  `Javascript/TopLevelAPI.js`; package entry points should only expose or
  re-export these APIs.
- Portable validation packages use Fable.Pyxpecto test cases. Prefer upstream
  structured results, but a focused ARCExpect adapter is allowed when waiting
  for a Pyxpecto release would leave JavaScript/Python without `Execute`.
- Do not add Expecto compatibility back to production ARCExpect. Immutable
  historical packages keep their exact older ARCExpect pins; new versions use
  Fable.Pyxpecto.

## AVPR dependency boundary

- `ValidationPackage.Model` is the canonical portable metadata, author, tag,
  CWL input, identity, and semantic-version model.
- `ValidationPackage.Codecs` is the canonical portable YAML/frontmatter and
  domain JSON implementation.
- `AVPRClient` is generated-only. Do not add domain helpers or hand-written
  mappings to it.
- `AVPRClient.Interop` owns generated-client DTO to portable-model mappings.
- Do not reference AVPR staging, registry service, database, or the retired
  `AVPRIndex` infrastructure from production code in this repository.
- During preview-package integration, pin exact prerelease versions, verify
  they are indexed before restore, and do not republish packages that are still
  indexing.
- AVPR package compatibility is covered by this repository's normal build,
  contract, packed-consumer, and release checks. Do not require AVPR CI to
  clone and rebuild this repository.
- The final target dependency graph uses matching native Model and Codecs
  artifacts: NuGet for .NET, npm for JavaScript, and PyPI for Python. Treat
  bundled Fable-generated dependency code as a bootstrap state only.
- All network-backed AVPR integration tests target
  `https://avpr-dev.nfdi4plants.org`, never the production registry. The dev
  instance carries candidate metadata fields such as CWL-style command-line
  inputs. Keep the endpoint explicit or injected in tests so production
  defaults remain unchanged.

## Package management and execution

Validation-package management is internal application infrastructure, not a
general-purpose portable authoring library. The former `ARCValidationPackages`
project has been absorbed into the CLI.

- Registry configuration, cache management, installation, and uninstallation
  belong in internal CLI package-management modules when migrated.
- FSI and `uv run` process execution belong in internal CLI package-runner
  modules.
- Preserve the existing application-data cache location and readable cache
  format during migration.
- Honor configured cache folders and registry endpoints, use atomic cache
  writes, use asynchronous HTTP with status-based error classification, and
  pass process arguments safely.
- Cover install, list, update, uninstall, and execution with injected/local
  endpoints or the explicit AVPR development instance, never the production
  registry.
- Keep CLI handlers focused on arguments, orchestration, exit codes, and
  presentation.

## CI and release safety

- Pushes and pull requests targeting `dev` or `release` run the cross-platform
  build/test workflow when source, tests, build logic, or workflows change.
- Pushes changing `src/arc-validate/**` or `Dockerfile` can publish a GHCR
  container after Linux and Windows tests pass.
- Documentation changes run verified polyglot samples and a strict site build.
  A `release` push or manual docs workflow dispatch can deploy GitHub Pages.
- Preserve least-privilege workflow permissions and deliberate action versions.
  Never print tokens, NuGet keys, registry credentials, or other secrets.
- Treat release-note and build-project edits as release-sensitive. Check the
  affected pack/version behavior before handing off.
- Before completing a change, report focused and solution-level checks run,
  cross-target checks run for portable code, and anything skipped because it
  requires unavailable network access or external services.
