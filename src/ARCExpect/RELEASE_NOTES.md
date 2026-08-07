## 7.0.0-preview.6 - 2026-08-05

- Organize ARCExpect sources by shared and target-specific concerns and expose
  explicit .NET, Python, and JavaScript top-level facades.
- Select metadata frontmatter handling from the compilation target so F# and
  Python package authors no longer pass or import a language discriminator.
- Accept native JavaScript payload objects and reject non-finite payload
  numbers consistently across .NET, JavaScript, and Python.
- Preserve validation declaration order and exception stack traces in JUnit,
  emit aggregate suite counts, and validate decoded summary counts.
- Correct default badge threshold selection for failed, partial, and fully
  successful validation results.

### 7.0.0-preview.5 - 2026-08-04

- Accept native JSON-compatible payload values at package boundaries:
  `IDictionary<string, obj>` on .NET and dictionaries, lists, and scalar values
  in Python, while retaining Thoth JSON as the shared output representation.
- Add verified payload documentation and reproducibly formatted summary JSON
  and JUnit artifacts for all documentation samples.

### 7.0.0-preview.4 - 2026-08-04

- Make Fable.Pyxpecto the single ARCExpect validation-package model on every
  target and remove the production Expecto compatibility dependency.
- Restore the high-level validation pipeline for portable packages, including
  the standard result layout and automatic JSON, JUnit, and badge creation on
  .NET and Python.
- Keep result combination and output encoding inside ARCExpect instead of
  requiring package authors to orchestrate individual writers.
- Retain individual summary, JUnit, and badge creation APIs for custom output
  workflows while keeping the shared orchestration bundle internal.

### 7.0.0-preview.3 - 2026-08-04

- Accept the runtime value of Python `PACKAGE_METADATA` frontmatter directly;
  package authors no longer need to reconstruct source-code triple quotes.
- Add verified F# and Python validation-package documentation samples and the
  MkDocs Material/fsdocs documentation scaffold.

### 7.0.0-preview.2 - 2026-08-04

- Add one cross-target `PackageArguments` API for the four standard package
  arguments and typed CWL-defined inputs, with native .NET, Node, and Python
  command-line readers.
- Reject reserved-name collisions and unknown, duplicate, missing required, or
  malformed values consistently across all three runtimes.

### 7.0.0-preview.1 - 2026-07-31

- Consolidate portable result/output contracts and the .NET compatibility
  implementation behind the single public ARCExpect package identity.
- Build the shared portable sources through separate .NET, JavaScript, and
  Python projects and produce `ARCExpect` NuGet plus `arcexpect` npm/wheel
  artifacts.
- Depend on native ValidationPackage.Model and ValidationPackage.Codecs
  distributions in npm and Python artifacts instead of bundling generated
  copies.
- Restore shared `Setup`, `ARCValidationPackage`, and top-level `Execute` APIs
  with Pyxpecto cases across .NET, JavaScript, and Python while retaining the
  Expecto runner and filesystem pipeline as .NET compatibility APIs.
- Keep Expecto, filesystem, ARC specification, and
  ARCTokenization/ControlledVocabulary APIs available only on the .NET target.
- Add optional source branch and commit provenance to summary JSON, JUnit
  properties, and non-rendered SVG metadata.
- Remove the unused ARCGraph, OboGraph, OBO.NET, Graphoscope, and Cytoscape.NET
  functionality instead of porting it to JavaScript and Python.

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
