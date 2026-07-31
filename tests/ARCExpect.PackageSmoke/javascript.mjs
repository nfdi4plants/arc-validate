import {
  Badge,
  CaseOutcome,
  CaseResult,
  JUnit,
  ValidationPackageSummary,
  ValidationResult,
  ValidationSummary
} from "arcexpect";
import {
  CommandInputBinding,
  CommandInputParameter,
  CommandInputType,
  CwlPrimitive,
  ValidationPackageMetadata
} from "validationpackage-model";

const result = ValidationResult.create([
  CaseResult.create(["packed"], CaseOutcome.passed())
]);
const metadata = ValidationPackageMetadata.create(
  "packed",
  "summary",
  "description",
  1,
  0,
  0,
  "FSharp"
);
metadata.Inputs = [
  CommandInputParameter.create(
    "arc-directory",
    CommandInputType.create(CwlPrimitive.String),
    CommandInputBinding.create(undefined, "--arc-directory", false)
  )
];
const packageSummary = ValidationPackageSummary.fromMetadata(metadata);
const summary = ValidationSummary.create(result, ValidationResult.create([]), packageSummary);

if (
  result.Passed !== 1 ||
  packageSummary.Version !== "1.0.0" ||
  metadata.Inputs[0].InputBinding.Prefix !== "--arc-directory" ||
  !ValidationSummary.toJson(summary).includes('"Passed":1') ||
  !Badge.Writer.toSvg(summary, "packed").includes("1/1") ||
  !JUnit.Writer.toXml(result).includes("<testcase")
) {
  throw new Error("Packed JavaScript ARCExpect consumer failed.");
}