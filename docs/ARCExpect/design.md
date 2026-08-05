# ARCExpect API design

ARCExpect provides one validation-package authoring model across .NET, Python,
and JavaScript. Public package authors work with `Setup`, `PackageArguments`,
Fable.Pyxpecto cases, and `Execute`; target-specific adapters handle runtime
details.

## Design principles

- **One package model:** metadata comes from AVPR frontmatter and validation
  cases use Fable.Pyxpecto on every target.
- **One-call standard execution:** `Execute.ValidationPipeline` runs cases and
  creates the summary JSON, JUnit report, and badge wherever the target has a
  filesystem adapter.
- **Composable operations:** pure encoders and individual output operations
  remain available for custom workflows.
- **Native boundaries:** package authors pass .NET dictionaries, Python
  dictionaries/lists/scalars, or JavaScript objects/arrays/scalars as payloads.
  ARCExpect converts them to its shared Thoth JSON representation.
- **No target namespaces:** `Common/`, `DotNet/`, `Python/`, and
  `Javascript/` organize implementation source only. The public identity is
  always ARCExpect/arcexpect.

## Metadata setup

Validation packages begin with `PACKAGE_METADATA` frontmatter and call:

```fsharp
let metadata = Setup.Metadata(PACKAGE_METADATA)
```

The installed target selects the language codec. .NET selects F# and Python
selects Python, so package authors do not import or pass a frontmatter-language
type. The codec-level language union remains in ValidationPackage.Codecs for
registry tooling that must parse multiple file types.

JavaScript validation-package frontmatter remains unavailable until AVPR
defines its JavaScript package format. ARCExpect fails explicitly instead of
silently treating JavaScript as F# or Python.

## Shared and target-specific behavior

| Capability | .NET | Python | JavaScript |
| --- | --- | --- | --- |
| Metadata setup | F# frontmatter | Python frontmatter | Not defined yet |
| Package construction | Yes | Yes | Yes, with existing metadata |
| Validation execution | F# async | Awaitable | Promise |
| Native payloads | Dictionaries/scalars | Dict/list/scalars | Object/array/scalars |
| Standard filesystem pipeline | Yes | Yes | Not defined yet |
| Pure JSON/JUnit/badge writers | Yes | Yes | Yes |
| CV/ARCTokenization helpers | Yes | No | No |

Target-native top-level implementations live in `DotNet/TopLevelAPI.fs`,
`Python/top_level_api.py`, and `Javascript/TopLevelAPI.js`. Package entry
points only expose those implementations.

## Output contract

The standard pipeline writes:

```text
.arc-validate-results/<name>@<version>/
  validation_summary.json
  validation_report.xml
  badge.svg
```

The shared output pipeline owns filenames and content generation. Filesystem
adapters only create directories and write the supplied content. Optional
source branch and commit provenance is carried consistently by all three
formats.

## .NET compatibility APIs

ControlledVocabulary, ARCTokenization, ARC specification, and semantic
`Validate.Param` helpers remain .NET-specific. They are compatibility and
domain-convenience APIs, not the foundation of the portable ARCExpect design.
