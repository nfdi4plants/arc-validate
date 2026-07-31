from arcexpect import (
    Badge,
    CaseOutcome,
    CaseResult,
    JUnit,
    ValidationPackageSummary,
    ValidationResult,
    ValidationSummary,
)
from validation_package_model import (
    CommandInputBinding,
    CommandInputParameter,
    CommandInputType,
    CwlPrimitive,
    ValidationPackageMetadata,
)


result = ValidationResult.create([
    CaseResult.create(["packed"], CaseOutcome.passed())
])
metadata = ValidationPackageMetadata.create(
    "packed",
    "summary",
    "description",
    1,
    0,
    0,
    "FSharp",
)
metadata.Inputs = [
    CommandInputParameter.create(
        "arc-directory",
        CommandInputType.create(CwlPrimitive.String),
        CommandInputBinding.create(None, "--arc-directory", False),
    )
]
package_summary = ValidationPackageSummary.from_metadata(metadata)
summary = ValidationSummary.create(
    result, ValidationResult.create([]), package_summary
)

if (
    result.Passed != 1
    or package_summary.Version != "1.0.0"
    or metadata.Inputs[0].InputBinding.Prefix != "--arc-directory"
    or '"Passed":1' not in ValidationSummary.to_json(summary)
    or "1/1" not in Badge.Writer.to_svg(summary, "packed")
    or "<testcase" not in JUnit.Writer.to_xml(result)
):
    raise RuntimeError("Packed Python ARCExpect consumer failed.")