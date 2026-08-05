PACKAGE_METADATA = """
---
Name: payload-validation
Summary: A validation package with calculated output data
Description: Shows how package computations become validation-summary payload JSON.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
---
"""

# /// script
# dependencies = [
#   "arcexpect==7.0.0a6",
# ]
# ///

import asyncio
from pathlib import Path

from arcexpect import (
    Execute,
    PackageArguments,
    Setup,
    test_case,
)

metadata = Setup.Metadata(PACKAGE_METADATA)

arguments = PackageArguments.from_command_line(metadata)
files_checked = sum(
    1 for path in Path(arguments.ArcDirectory).rglob("*") if path.is_file()
)


def validate_arc_contains_files():
    if files_checked <= 0:
        raise RuntimeError("The ARC contains no files.")


validation_package = Setup.ValidationPackage(
    metadata,
    [test_case("the ARC contains files", validate_arc_contains_files)],
)

payload = {
    "Metrics": {
        "FilesChecked": files_checked,
    },
    "Package": metadata.Name,
}

asyncio.run(
    Execute.validation_pipeline(
        validation_package,
        arguments=arguments,
        payload=payload,
    )
)

print(f"Recorded payload metrics for {files_checked} files.")
