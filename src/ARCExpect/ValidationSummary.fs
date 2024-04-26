namespace ARCExpect

open Expecto
open System.Text.Json
open System.Text.Json.Serialization
open System.IO

/// <summary>
/// Represents a brief summary of the result of validating an ARC against a set of validation cases.
/// </summary>
type ValidationResult = {
    HasFailures: bool
    Total: int
    Passed: int
    Failed: int
    Errored: int
    [<JsonIgnore>]
    OriginalRunSummary: Impl.TestRunSummary option
} with
    static member create(
        hasFailures: bool,
        total: int,
        passed: int,
        failed: int,
        errored: int,
        ?OriginalRunSummary: Impl.TestRunSummary
    ) = {
        HasFailures = hasFailures
        Total = total
        Passed = passed
        Failed = failed
        Errored = errored
        OriginalRunSummary = OriginalRunSummary
    }
    
    static member create (total: int, passed: int, failed: int, errored: int, ?OriginalRunSummary: Impl.TestRunSummary) =
        ValidationResult.create(
            hasFailures = (failed > 0 || errored > 0),
            total = total,
            passed = passed,
            failed = failed,
            errored = errored,
            ?OriginalRunSummary = OriginalRunSummary
        )

    static member ofExpectoTestRunSummary (summary: Impl.TestRunSummary) =

        let totalTests = summary.errored @ summary.failed @ summary.ignored @ summary.passed

        ValidationResult.create(
            total = totalTests.Length,
            passed = summary.passed.Length,
            errored = summary.errored.Length,
            failed = summary.failed.Length,
            OriginalRunSummary = summary
        )

/// <summary>
/// Represents a brief summary of a validation package. Should be expanded to include full package metadata in the future.
/// </summary>
type ValidationPackageSummary = {
    Name: string
    Version: string
    HookEndpoint: string option
} with
    static member create(
        name: string,
        version: string,
        ?HookEndpoint: string
    ) = {
        Name = name
        Version = version
        HookEndpoint = HookEndpoint
    }

/// <summary>
/// Represents a summary of the validation results of an ARC against a validation package containing critical and non-critical validation cases.
/// </summary>
type ValidationSummary = {
    Critical: ValidationResult
    NonCritical: ValidationResult
    ValidationPackage: ValidationPackageSummary
} with
    static member create(
        critical: ValidationResult,
        nonCritical: ValidationResult,
        validationPackage: ValidationPackageSummary
    ) = {
        Critical = critical
        NonCritical = nonCritical
        ValidationPackage = validationPackage
    }
    static member ofExpectoTestRunSummaries (
        criticalSummary: Impl.TestRunSummary,
        nonCriticalSummary: Impl.TestRunSummary,
        package: ValidationPackageSummary
    ) =
        ValidationSummary.create(
            critical = ValidationResult.ofExpectoTestRunSummary criticalSummary,
            nonCritical = ValidationResult.ofExpectoTestRunSummary nonCriticalSummary,
            validationPackage = package
        )
    
    static member toJson (summary: ValidationSummary) =
        JsonSerializer.Serialize(summary, JsonOptions.options)

    static member fromJson (json: string) =
        JsonSerializer.Deserialize<ValidationSummary>(json, JsonOptions.options)

    static member writeJson (path: string) (summary: ValidationSummary) =
        File.WriteAllText(path, ValidationSummary.toJson summary)