import {
  Badge,
  CaseOutcome,
  CaseResult,
  JUnit,
  ValidationPackageSummary,
  ValidationResult,
  ValidationSummary
} from "arcexpect";

const result = ValidationResult.create([
  CaseResult.create(["packed"], CaseOutcome.passed())
]);
const packageSummary = ValidationPackageSummary.create("packed", "1.0.0", "summary", "description");
const summary = ValidationSummary.create(result, ValidationResult.create([]), packageSummary);

if (
  result.Passed !== 1 ||
  !ValidationSummary.toJson(summary).includes('"Passed":1') ||
  !Badge.Writer.toSvg(summary, "packed").includes("1/1") ||
  !JUnit.Writer.toXml(result).includes("<testcase")
) {
  throw new Error("Packed JavaScript ARCExpect consumer failed.");
}
