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