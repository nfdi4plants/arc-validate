# arc-validate
Test project and docker container for the ARC validation workflow.

## ISA test description

- **Schema**: Tests about the ISA schema format correctness E.g.:
  - _Is there an investigation?_
- **Semantic**: Tests about semantic compliance to ARC specification E.g.:
  - _Do all terms have identifiers?_
  - _Is the ARC CWL-compliant?_
- **Plausibility**: Tests about scientific plausibility E.g.:
  - _Is there a Factor?_
  - _Does the ISA object make sense from a scientific point of view?_

## Build Docker container 

In the repo root, run `docker build -t arc-validate .`

## Use the container

In repo root use `docker run -it arc-validate bash`.
Navigate to repo root inside the Docker container: `cd ../opt/arc-validate/`.

### Running ARC validation

The container will contain a `arc-validate.sh` script that will execute the test project:

- Execute `arc-validate.sh` 
- The test project will look for ARC-related files and folders in {$ARC_PATH} and run several test on them to validate the ARC
- The test results are then written to `{$ARC_PATH}/arc-validate-results.xml` (in the container)

So there are 2 easy ways to use the container:
- mount a local directory with `test.txt` into the container and run `arc-validate.sh`, e.g.: `docker run -d --mount type=bind,source={YOUR_ARC_FOLDER_HERE},target=/arc arc-validate arc-validate.sh`
- use it as a base image and use `arc-validate.sh` directly.

### Creating the badge

The container will contain a `create-badge.py` script that will create the arc quality badge:

- The script will parse `arc-validate-results.xml` in {$ARC_PATH}
- The script will create a badge displaying passed/failed tests in the working directory as `{$ARC_PATH}/arc-quality.svg`, e.g: ![](./test/arc-quality.svg)

So there are 2 easy ways to use the script in the container:
- mount a local directory with `arc-validate-results.xml` (e.g. after runnning `arc-validate.sh` as specified above) into the container and run `create-badge.py`, e.g.: `docker run -d --mount type=bind,source={YOUR_ARC_FOLDER_HERE},target=/arc arc-validate create-badge.py`
- use it as a base image and use `create-badge.py` directly.