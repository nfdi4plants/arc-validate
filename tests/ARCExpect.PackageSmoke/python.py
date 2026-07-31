from arcexpect import (
    Badge,
    CaseOutcome,
    CaseResult,
    JUnit,
    ValidationPackageSummary,
    ValidationResult,
    ValidationSummary,
)


result = ValidationResult.create([
    CaseResult.create(["packed"], CaseOutcome.passed())
])
package_summary = ValidationPackageSummary.create(
    "packed", "1.0.0", "summary", "description"
)
summary = ValidationSummary.create(
    result, ValidationResult.create([]), package_summary
)

if (
    result.Passed != 1
    or '"Passed":1' not in ValidationSummary.to_json(summary)
    or "1/1" not in Badge.Writer.to_svg(summary, "packed")
    or "<testcase" not in JUnit.Writer.to_xml(result)
):
    raise RuntimeError("Packed Python ARCExpect consumer failed.")
