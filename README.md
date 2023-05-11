# arc-validate

Base repository to validate [ARCs](https://github.com/nfdi4plants/ARC-specification/blob/main/ARC%20specification.md).  
This repository provides containerized validation workflows for usage in [DataHUB](https://git.nfdi4plants.org/explore)-CI jobs.  
Uses [Expecto](https://github.com/haf/expecto) and [ArcGraphModel](https://github.com/nfdi4plants/ArcGraphModel) to run unit tests that test ARCs according to the ARC-specification and the [ISA.NET](https://github.com/nfdi4plants/ISADotNet) model.

## Workflow of the validation

The project executes several specified unit tests on a given path to an ARC folder and creates a JUnit summary file containing the test results. Afterwards runs a python script that reads the summary file and creates an SVG badge according to the test results.  
Tests are separated hierarchically into several test lists to tackle both ARC specification as well as ISA.NET standard.  
This structure is also depicted in the _testcase name_ in the summary file.

_Filesystem_ tests treat ARC specification-related requirements on ARCs regarding filesystem structure (i.e., the presence and content of specific files and folders) while _ISA_ tests cover both ARC specification as well as the ISA standard (in the form of ISA.NET requirements).

### Filesystem tests

_(WIP)_

### ISA tests

- **Schema**: Tests about the ISA schema format correctness. E.g.:
  - _Is there an investigation?_
- **Semantic**: Tests about semantic compliance to ARC specification. E.g.:
  - _Do all terms have identifiers?_
  - _Is the ARC CWL-compliant?_
- **Plausibility**: Tests about scientific plausibility. E.g.:
  - _Is there a Factor?_
  - _Does the ISA object make sense from a scientific point of view?_

## Structure

```mermaid
flowchart TD
subgraph "Toplevel (Alternative)"
    G[FilesystemEntries] 
	H[Filesystem.DataModel]
	I[CvParam list]
    J[ValidationResult list]
    K[JUnit XML ResultsFile]
    L[Badge]

	G --->|Reader| H
	H --->|MetadataFilesToCvParams| I
	H --->|Validation| J
	I --->|Validation| J
	J --->|Writer| K
	K --->|CreateBadge| L

end

subgraph Toplevel
    A[FilesystemEntries]
	B[Filesystem.DataModel]
	C[CvParam list]
	D[ValidationResult list]
	E[JUnit XML ResultsFile]
	F[Badge]
	A -->|Reader| B
	B -->|MetaDataFilesToCvParams| C
    C -->|Validation| D
    D -->|Writer| E
    E -->|CreateBadge| F
end
```

## Requirements:

### Filesystem.DataModel

1. Shall represent the ARCs required file and folder structure. Required are top level folders as fields (e.g. Assays, Runs, ...)

### Reader:

#### Filesystem.DataModel

Representation of filesystem structure (possibly `Filesystem.DataModel` in ARC.Core?)

#### CvParam parsing

Metadata parsing according to ARC specification (ISA.XLSX files, CWL files)
  - Depending on the type of input (e.g. AnnotationTable, Investigation metadata section), the reader must parse the content respectively:
    - Parse metadata section in Assay/Study/Investigation: Search for sheet with respective name ("Study"/"Assay"/"isa_investigation") and parse the content of the row-based table (key-value pairs) into CvParams
    - Parse AnnotationTable in Assay/Study: Search for all tables containing "annotationTable" in their name and parse the content of the column-based table (key-value pairs) into CvParams

## Develop

If you'd like to contribute, please follow the following steps (basic knowledge about Docker is helpful but not required):

### Build Docker container 

Start your Docker environment (e.g. Docker Desktop).  
In the repo root, run `docker build -t arc-validate .`

### How to look into the container

In repo root use `docker run -it arc-validate bash`.
Navigate to repo root inside the Docker container: `cd ../opt/arc-validate/`.

### Running ARC validation

The container will contain an `arc-validate.sh` script that will execute the test project:

- Execute `arc-validate.sh` 
- The test project will look for ARC-related files and folders in {$ARC_PATH} and run several test on them to validate the ARC
- The test results are then written to `{$ARC_PATH}/arc-validate-results.xml` (in the container)

So there are 2 easy ways to use the container:
1. Mount a local ARC into the container and run `arc-validate.sh`, e.g.: `docker run -d --mount type=bind,source={PATH_TO_YOUR_ARC_FOLDER_HERE},target=/arc arc-validate arc-validate.sh`  
-or-
2. Use it as a base image and use `arc-validate.sh` directly.

### Creating the badge

The container will contain a `create-badge.py` script that will create the arc quality badge:

- The script will parse `arc-validate-results.xml` in {$ARC_PATH}
- The script will create a badge displaying passed/failed tests in the working directory as `{$ARC_PATH}/arc-quality.svg`, e.g: ![](./test/arc-quality.svg)
- *Note:* Currently, tests that ***error*** (in contrast to tests that ***fail***) are **ignored for calculation**.

So there are 2 easy ways to use the script in the container:
1. Mount a local directory with `arc-validate-results.xml` (e.g. after runnning `arc-validate.sh` as specified above) into the container and run `create-badge.py`, e.g.: `docker run -d --mount type=bind,source={PATH_TO_YOUR_ARC_FOLDER_HERE},target=/arc arc-validate create-badge.py`  
-or-
2. Use it as a base image and use `create-badge.py` directly.