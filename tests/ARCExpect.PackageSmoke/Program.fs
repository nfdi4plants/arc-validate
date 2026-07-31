module ARCExpect.PackageSmoke.Program

open ARCExpect
open ARCExpect.Badge
open Fable.Pyxpecto
open ValidationPackage.Model

[<EntryPoint>]
let main _ =
    let result =
        CaseResult.create([| "packed" |], CaseOutcome.passed())
        |> fun caseResult -> ValidationResult.create([| caseResult |])

    let metadata =
        ValidationPackageMetadata.create(
            "packed",
            "summary",
            "description",
            1,
            0,
            0,
            "FSharp"
        )
    let validationPackage =
        Setup.ValidationPackage(
            metadata,
            CriticalValidationCases = [| testCase "packed execute" <| fun () -> () |]
        )
    let executed =
        Execute.Validation(validationPackage)
        |> Async.RunSynchronously

    let package = ValidationPackageSummary.fromMetadata metadata
    let summary = ValidationSummary.create(result, ValidationResult.create(Array.empty), package)
    let json = ValidationSummary.toJson summary
    let badge = Writer.toSvg(summary, "packed")

    if
        result.Passed <> 1
        || executed.Critical.Passed <> 1
        || not (json.Contains("\"Passed\":1"))
        || not (badge.Contains("1/1"))
    then
        failwith "Packed portable consumer failed."

    0