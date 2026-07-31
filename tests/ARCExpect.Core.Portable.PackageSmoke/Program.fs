module ARCExpect.Core.Portable.PackageSmoke.Program

open ARCExpect
open ARCExpect.Badge

[<EntryPoint>]
let main _ =
    let result =
        CaseResult.create([| "packed" |], CaseOutcome.passed())
        |> fun caseResult -> ValidationResult.create([| caseResult |])

    let package = ValidationPackageSummary.create("packed", "1.0.0", "summary", "description")
    let summary = ValidationSummary.create(result, ValidationResult.create(Array.empty), package)
    let json = ValidationSummary.toJson summary
    let badge = Writer.toSvg(summary, "packed")

    if result.Passed <> 1 || not (json.Contains("\"Passed\":1")) || not (badge.Contains("1/1")) then
        failwith "Packed portable consumer failed."

    0
