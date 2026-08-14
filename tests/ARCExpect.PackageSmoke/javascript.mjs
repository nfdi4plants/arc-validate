import {
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
  ptestCase,
  testCase,
  testCaseAsync
} from "@nfdi4plants/arcexpect";
import {
  CommandInputBinding,
  CommandInputParameter,
  CommandInputType,
  CwlPrimitive,
  ValidationPackageMetadata
} from "@nfdi4plants/validationpackage-model";

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
    "echo",
    CommandInputType.create(CwlPrimitive.String, true),
    CommandInputBinding.create(undefined, "--echo")
  )
];
const packageArguments = PackageArguments.fromCommandLine(metadata);
const packageSummary = ValidationPackageSummary.fromMetadata(metadata);
const validationPackage = Setup.ValidationPackage(
  metadata,
  [
    testCase("native pass", () => {}),
    testCaseAsync("native async pass", async () => {})
  ],
  [ptestCase("native pending", () => {})]
);
const executionSummary = await Execute.validation(
  validationPackage,
  {
    payload: {
      Runtime: "javascript",
      Nested: { Count: 2 }
    },
    arguments: packageArguments
  }
);
const summary = ValidationSummary.create(result, ValidationResult.create([]), packageSummary);

if (
  result.Passed !== 1 ||
  packageSummary.Version !== "1.0.0" ||
  metadata.Inputs[0].InputBinding.Prefix !== "--echo" ||
  packageArguments.ArcDirectory !== "packed-arc" ||
  packageArguments.OutputDirectory !== "packed-out" ||
  packageArguments.TryGetString("echo") !== "literal; $(not-executed)" ||
  executionSummary.Critical.Passed !== 2 ||
  executionSummary.NonCritical.Skipped !== 1 ||
  !ValidationSummary.toJson(executionSummary).includes('"Runtime":"javascript"') ||
  !ValidationSummary.toJson(executionSummary).includes('"Count":2') ||
  !ValidationSummary.toJson(summary).includes('"Passed":1') ||
  !Badge.Writer.toSvg(summary, "packed").includes("1/1") ||
  !JUnit.Writer.toXml(result).includes("<testcase")
) {
  throw new Error("Packed JavaScript ARCExpect consumer failed.");
}
