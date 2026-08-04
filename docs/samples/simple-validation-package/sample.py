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
#   "arcexpect==7.0.0a3",
# ]
# ///

import asyncio
from pathlib import Path

from arcexpect import (
    Badge,
    Execute,
    FrontmatterLanguage,
    JUnit,
    PackageArguments,
    RunSummary,
    Setup,
    ValidationSummary,
    test_case,
)

metadata = Setup.Metadata(
    PACKAGE_METADATA,
    FrontmatterLanguage.PythonFrontmatter,
)

arguments = PackageArguments.from_command_line(metadata)


def validate_package_name():
    if metadata.Name != "simple-validation":
        raise RuntimeError("Unexpected package name.")


validation_package = Setup.ValidationPackage(
    metadata,
    [test_case("the package has a stable name", validate_package_name)],
)

summary = asyncio.run(
    Execute.validation(validation_package, arguments=arguments)
)

if summary.Critical.HasFailures:
    raise RuntimeError("The validation package reported a critical failure.")

output_directory = Path(arguments.OutputDirectory)
output_directory.mkdir(parents=True, exist_ok=True)
combined = RunSummary.combine([summary.Critical, summary.NonCritical])

(output_directory / "validation_summary.json").write_text(
    ValidationSummary.to_json(summary),
    encoding="utf-8",
)
(output_directory / "validation_report.xml").write_text(
    JUnit.Writer.to_xml(
        combined,
        SuiteName=metadata.Name,
        SourceBranch=summary.SourceBranch,
        SourceCommitHash=summary.SourceCommitHash,
    ),
    encoding="utf-8",
)
(output_directory / "badge.svg").write_text(
    Badge.Writer.to_svg(summary, metadata.Name),
    encoding="utf-8",
)

print(f"Passed {summary.Critical.Passed}/{summary.Critical.Total} critical tests.")
