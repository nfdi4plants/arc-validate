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