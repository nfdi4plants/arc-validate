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
minimum_files = arguments.GetInt("minimum-files")


def validate_minimum_files():
    if minimum_files <= 0:
        raise RuntimeError("minimum-files must be positive.")


def validate_strict_label():
    if arguments.GetBoolean("strict") and arguments.TryGetString("label") is None:
        raise RuntimeError("Strict mode requires a label.")


validation_package = Setup.ValidationPackage(
    metadata,
    [
        test_case("minimum-files is positive", validate_minimum_files),
        test_case("strict mode has a label", validate_strict_label),
    ],
)

summary = asyncio.run(
    Execute.validation(validation_package, arguments=arguments)
)

if summary.Critical.HasFailures:
    raise RuntimeError("The configurable validation package failed.")

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

print(
    f"Validated {arguments.ArcDirectory}; "
    f"results belong in {arguments.OutputDirectory}."
)
