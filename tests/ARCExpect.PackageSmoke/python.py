import asyncio

from arcexpect import (
    Badge,
    CaseOutcome,
    CaseResult,
    Execute,
    JUnit,
    ValidationPackageSummary,
    Setup,
    ValidationResult,
    ValidationSummary,
    ptest_case,
    test_case,
    test_case_async,
)
from validation_package_model import (
    CommandInputBinding,
    CommandInputParameter,
    CommandInputType,
    CwlPrimitive,
    ValidationPackageMetadata,
)


async def native_async_pass():
    return None

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
validation_package = Setup.ValidationPackage(
    metadata,
    [
        test_case("native pass", lambda: None),
        test_case_async("native async pass", native_async_pass),
    ],
    [ptest_case("native pending", lambda: None)],
)
execution_summary = asyncio.run(Execute.validation(validation_package))
summary = ValidationSummary.create(
    result, ValidationResult.create([]), package_summary
)

if (
    result.Passed != 1
    or package_summary.Version != "1.0.0"
    or metadata.Inputs[0].InputBinding.Prefix != "--arc-directory"
    or execution_summary.Critical.Passed != 2
    or execution_summary.NonCritical.Skipped != 1
    or '"Passed":1' not in ValidationSummary.to_json(summary)
    or "1/1" not in Badge.Writer.to_svg(summary, "packed")
    or "<testcase" not in JUnit.Writer.to_xml(result)
):
    raise RuntimeError("Packed Python ARCExpect consumer failed.")
