PACKAGE_METADATA = """
---
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: packed
Summary: summary
Description: description
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
Inputs:
  - id: echo
    type: string?
    inputBinding:
      prefix: --echo
---
"""

import asyncio
import tempfile
from pathlib import Path

from arcexpect import (
    Badge,
    CaseOutcome,
    CaseResult,
    Execute,
    JUnit,
    PackageArguments,
    ValidationPackageSummary,
    Setup,
    ValidationResult,
    ValidationSummary,
    ptest_case,
    test_case,
    test_case_async,
)
async def native_async_pass():
    return None

result = ValidationResult.create([
    CaseResult.create(["packed"], CaseOutcome.passed())
])
metadata = Setup.Metadata(PACKAGE_METADATA)
package_arguments = PackageArguments.from_command_line(metadata)
package_summary = ValidationPackageSummary.from_metadata(metadata)
validation_package = Setup.ValidationPackage(
    metadata,
    [
        test_case("native pass", lambda: None),
        test_case_async("native async pass", native_async_pass),
    ],
    [ptest_case("native pending", lambda: None)],
)
execution_summary = asyncio.run(
    Execute.validation(
        validation_package,
        payload={"Runtime": "python", "Count": 2},
        arguments=package_arguments,
    )
)
summary = ValidationSummary.create(
    result, ValidationResult.create([]), package_summary
)

with tempfile.TemporaryDirectory(dir=Path.cwd()) as output_directory:
    summary_path = Path(output_directory) / "validation_summary.json"
    junit_path = Path(output_directory) / "validation_report.xml"
    badge_path = Path(output_directory) / "badge.svg"
    Execute.summary_creation(summary_path, summary)
    Execute.junit_report_creation(junit_path, summary)
    Execute.badge_creation(badge_path, summary, "packed")

    individual_outputs_exist = (
        summary_path.is_file()
        and junit_path.is_file()
        and badge_path.is_file()
    )

if (
    result.Passed != 1
    or package_summary.Version != "1.0.0"
    or metadata.ProgrammingLanguage != "Python"
    or metadata.Inputs[0].InputBinding.Prefix != "--echo"
    or package_arguments.ArcDirectory != "packed-arc"
    or package_arguments.OutputDirectory != "packed-out"
    or package_arguments.TryGetString("echo") != "literal; $(not-executed)"
    or execution_summary.Critical.Passed != 2
    or execution_summary.NonCritical.Skipped != 1
    or '"Runtime":"python"' not in ValidationSummary.to_json(execution_summary)
    or '"Count":2' not in ValidationSummary.to_json(execution_summary)
    or '"Passed":1' not in ValidationSummary.to_json(summary)
    or "1/1" not in Badge.Writer.to_svg(summary, "packed")
    or "<testcase" not in JUnit.Writer.to_xml(result)
    or not individual_outputs_exist
):
    raise RuntimeError("Packed Python ARCExpect consumer failed.")
