## 7.0.0-preview.4 - 2026-08-14

- Consolidate portable result and output contracts behind one public ARCExpect
  package identity with separate .NET, JavaScript, and Python projects and
  top-level facades.
- Use Fable.Pyxpecto as the cross-target validation-package test model and keep
  filesystem, ARC specification, and ControlledVocabulary functionality at the
  .NET boundary.
- Restore shared `Setup`, `ARCValidationPackage`, and high-level `Execute` APIs,
  including the complete validation pipeline and individual summary, JUnit,
  and badge creation functions.
- Select frontmatter handling from the compilation target, including direct
  support for Python `PACKAGE_METADATA`, without requiring authors to pass a
  language discriminator.
- Add one cross-target `PackageArguments` API for the standard package
  arguments and typed CWL inputs, rejecting reserved-name collisions and
  unknown, duplicate, missing, or malformed arguments consistently.
- Accept native JSON-compatible payloads on each target while retaining Thoth
  JSON as the shared output representation and rejecting non-finite numbers.
- Add optional source branch and commit provenance to summary JSON, JUnit
  properties, and non-rendered SVG metadata.
- Preserve validation declaration order and exception stack traces in JUnit,
  emit aggregate suite counts, validate decoded summary counts, and correct
  default badge thresholds for failed, partial, and successful results.
- Depend on the native ValidationPackage.Model and ValidationPackage.Codecs
  preview.4 distributions instead of bundling generated dependency copies.
- Align package-argument parsing with preview.4 CWL bindings: every declaration
  has a non-empty prefix, booleans remain flags, and other values consume the
  following process argument.
- Reuse the portable Model validator as the single declaration contract, give
  declared prefixes precedence over optional long standard aliases, and retain
  exact-token argument parsing on .NET, JavaScript, and Python.
- Keep the ARCExpect assembly identity aligned with its package version when it
  is built through the CLI project graph.
- Add verified F# and Python validation-package guides with reproducibly
  generated and formatted summary JSON, JUnit, and badge artifacts.
- Remove the unused ARCGraph, OboGraph, OBO.NET, Graphoscope, Cytoscape.NET, and
  production Expecto compatibility functionality.
- Replace legacy token-based release targets with one verified artifact set
  and independent NuGet, npm, and PyPI trusted-publishing jobs, so a registry
  failure does not cancel publication to the other registries.

### 6.0.0 - (Released 2025-12-04)

- Support multiple programming languages
- Remove preview package APIs

### 5.0.1 - (Released 2025-09-26)

- Fix error with specification failing for multiple workflow files per workflow
- Update ORCID pattern and check functions

### 5.0.0 - (Released 2025-06-06)

- Add functionality to add arbitrary JSON payload to the resulting `validation_summary.json` file.

### 4.0.1 - (Released 2024-08-07)

- Add `ContainsNonKeyParamWithTerm` validation function

### 4.0.0 - (Released 2024-06-21)

- Support full semantic versions for ValidationPackageMetadata in the `ARCExpect` API.

- Use SemVer prerelease suffix for `arc_specification @2.0.0-draft_`

### 3.0.1 - (Released 2024-04-30)

Update pinned dependencies

### 3.0.0 - (Released 2024-04-30)

Remove separate `CQCHookEndpoint` because it is now directly contained in package metadata.

### 2.0.0 - (Released 2024-04-30)

Major rework and improvements of the **ARCExpect API**. The main changes are:

- Built-in mechanisms for validating ARCs against the ARC specification

- Built-in validation package to validate against ARC spec v2.0.0-draft

- **New Validate functions**:
  - `SatisfiesPredicate` for `Param` and `ParamCollection`

- **New `Setup` and `Execute` functions**:
  - `Setup` can be used in validation package code to aggregate package metadata, even from frontmatter yaml.
  - `Execute` now creates a spec v2 compliant output folder with the junit report, badge, and a new json report.

- **Addition of `Validation Summary` output**:
  - In addition to JUnit report and badge creation, a json file combining the test results and validation package metadata is created by `Execute.*` functions.
  - This file is intended to be used for further processing of the validation results, e.g. in a CQC pipeline.


### 1.0.1 - (Released 2024-02-27)

Add Pipeline execution function that generates ARC-spec v2 draft compliant output folder.

### 1.0.0 - (Released 2024-01-12)

First release of ARCExpect API as a package. Initially, it was only used in the `arc-validate` CLI tool.

`ARCExpect` offers easy to use and understand APIs to create and execute validation cases. The main intention of this library is offering a framework for validation of [ControlledVocabulary](https://github.com/nfdi4plants/ARCTokenization) tokens in **Research Data management (RDM)** based on testing principles from the world of software development:
- a `validation case` is the equivalent of a unit test
- `Validate.*` functions are the equivalent of `Assert.*`, `Expect.*`, or equivalent functions
- A `BadgeCreation` API based on [AnyBadge.NET](https://github.com/kMutagene/AnyBadge.NET) enables creation of badges that visualize the validation results.
- export of the validation results as a `junit` xml file enables further integration into e.g. CI/CD pipelines

**User-facing APIs:**

- `Validate` :
  - validate **ControlledVocabulary** tokens, e.g. for their compliance with a reference ontology or for the type and shape of annotated value
- `BadgeCreation`:
  - Create and style small .svg images that visualize the validation results of a validation suite

**Additional APIs:**

- `OBOGraph`
  - Create, complete, and query the graph representation of an ontology
- `ARCGraph`
  - Create, complete, and query a graph of ControlledVocabulary tokens based on the relations in a reference ontology
