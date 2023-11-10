# arc-validate

arc-validate is a CLI tool to validate [ARCs](https://github.com/nfdi4plants/ARC-specification/blob/main/ARC%20specification.md).

This repository provides containerized validation workflows for usage in [DataHUB](https://git.nfdi4plants.org/explore)-CI jobs.  

**This is the v2 branch of arc-validate that currently undergoes significant changes.**

## Project aim

Validation of ARCs based on:
- [**ARCTokenization**](): Structural ontologies for file formats (for parsing/tokenizing files): INVMSO, STDMSO, ASSMSO
- [**OBO.NET]()**:
    - parsing ontologies, generation of **OBO graphs** based on ontology term relation
    - code genearation of ontology modules with accessible terms
- [**ARCGraph**](): Graph representation of file content based on structural ontologies via **OBO graph**
- [**Graph-based**]() completion of File content (missing cells -> empty tokens) via **ARCGraph**
- [**ARCExpect**](): Expecto-like API for creating unit tests for validation
- [**Validation Packages**](): API for installing and executing additional validation packages

## Project layout

```mermaid
flowchart TD

ControlledVocabulary("<b>ControlledVocabulary:</b><br>Data model for CVs")
ARCTokenization("<b>ARCTokenization:</b><br>Tokenization of ARCs into CVs")
OBO.NET("<b>OBO.NET:</b><br>OBO Ontology data model and parsing")
ARCGraph("<b>ARCGraph:</b><br>Graph based on structural ontologies")
ARCExpect("<b>ARCExpect:</b><br>Expecto-like API for validation")
ARCValidationPackages("<b>ARCValidationPackages:</b><br>API for additional validation packages")
arc-validate("<b>arc-validate:</b><br>validation CLI tool")

arc-validate --depends on--> ARCExpect
arc-validate --depends on--> ARCValidationPackages
ARCTokenization --depends on--> ControlledVocabulary
ARCTokenization --depends on--> OBO.NET
ARCExpect --depends on--> ARCGraph
ARCExpect --depends on--> ARCTokenization
ARCGraph --depends on--> ARCTokenization
ARCGraph --depends on--> OBO.NET
```