PACKAGE_METADATA = """
---
Name: configurable-validation
Summary: A validation package with typed inputs
Description: Reads standard and CWL-defined arguments through ARCExpect.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
Inputs:
  - id: strict
    type: boolean
    inputBinding:
      prefix: --strict
      position: 0
      separate: true
  - id: minimum-files
    type: int
    inputBinding:
      prefix: --minimum-files
      position: 0
      separate: true
  - id: label
    type: string?
    inputBinding:
      prefix: --label
      position: 0
      separate: true
---
"""

# /// script
# dependencies = [
#   "arcexpect==7.0.0a6",
# ]
# ///

import asyncio

from arcexpect import (
    Execute,
    PackageArguments,
    Setup,
    test_case,
)

metadata = Setup.Metadata(PACKAGE_METADATA)

arguments = PackageArguments.from_command_line(metadata)
minimum_files = arguments.GetInt("minimum-files")
strict = arguments.GetBoolean("strict")
label = arguments.TryGetString("label")


def validate_minimum_files():
    if minimum_files <= 0:
        raise RuntimeError("minimum-files must be positive.")


def validate_strict_label():
    if strict and label is None:
        raise RuntimeError("Strict mode requires a label.")


validation_package = Setup.ValidationPackage(
    metadata,
    [
        test_case("minimum-files is positive", validate_minimum_files),
        test_case("strict mode has a label", validate_strict_label),
    ],
)

payload = {
    "Strict": strict,
    "MinimumFiles": minimum_files,
}

if label is not None:
    payload["Label"] = label

asyncio.run(
    Execute.validation_pipeline(
        validation_package,
        arguments=arguments,
        payload=payload,
    )
)

print(
    f"Validated {arguments.ArcDirectory}; "
    f"results belong in {arguments.OutputDirectory}."
)
