PACKAGE_METADATA = """
---
Name: simple-validation
Summary: A minimal ARCExpect package
Description: Shows YAML frontmatter, a critical test, and portable execution.
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

from arcexpect import (
    Execute,
    PackageArguments,
    Setup,
    test_case,
)

metadata = Setup.Metadata(PACKAGE_METADATA)

arguments = PackageArguments.from_command_line(metadata)


def validate_package_name():
    if metadata.Name != "simple-validation":
        raise RuntimeError("Unexpected package name.")


validation_package = Setup.ValidationPackage(
    metadata,
    [test_case("the package has a stable name", validate_package_name)],
)

asyncio.run(
    Execute.validation_pipeline(validation_package, arguments=arguments)
)

print(f"Validation outputs written below {arguments.OutputDirectory}.")
