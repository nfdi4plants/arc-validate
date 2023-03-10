# arc-validate
Test project and docker container for the arc validation workflow

## build docker container 

In the repo root, run `docker build -t arc-validate .`

## use the container

### running arc validation

The container will contain a `arc-validate.sh` script that will execute the test project:

- Execute `arc-validate.sh` In the working directory of the container
- The test project will currently look for a `test.txt` file in the working directory and run these 2 tests on the file:
  - does it exist?
  - does it contain the text "yes"?
- The test results are then written to `arc-validate-results.xml` (in the container)

So there are 2 easy ways to use the container:
- mount a local directory with `test.txt` into the container and run `arc-validate.sh`, e.g.: `docker run -d --mount type=bind,source={YOUR_ARC_FOLDER_HERE},target=/arc arc-validate arc-validate.sh`
- use it as a base image and use `arc-validate.sh` directly.

### creating the badge

The container will contain a `create-badge.py` script that will create the arc quality badge:

- the script will parse `arc-validate-results.xml` in the working directory
- the script will create a badge displaying passed/failed tests in the working directory as `arc-quality.svg`, e.g: ![](./test/arc-quality.svg)

So there are 2 easy ways to use the script in the container:
- mount a local directory with `arc-validate-results.xml` (e.g. after runnning `arc-validate.sh` as specified above) into the container and run `create-badge.py`, e.g.: `docker run -d --mount type=bind,source={YOUR_ARC_FOLDER_HERE},target=/arc arc-validate create-badge.py`
- use it as a base image and use `create-badge.py` directly.

