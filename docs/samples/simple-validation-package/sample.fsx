let [<Literal>]PACKAGE_METADATA = """(*
---
Name: simple-validation
Summary: A minimal ARCExpect package
Description: Shows YAML frontmatter, a critical test, and portable execution.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
---
*)"""

#r "nuget: ARCExpect, 7.0.0-preview.3"

open System.IO
open ARCExpect
open Fable.Pyxpecto

let metadata =
    Setup.Metadata(
        PACKAGE_METADATA,
        FrontmatterLanguage.FSharpFrontmatter
    )

let arguments = PackageArguments.fromCommandLine(metadata)

let validationPackage =
    Setup.ValidationPackage(
        metadata,
        CriticalValidationCases = [|
            testCase "the package has a stable name" <| fun () ->
                Expect.equal metadata.Name "simple-validation" "package name"
        |]
    )

let summary =
    Execute.Validation(validationPackage, Arguments = arguments)
    |> Async.RunSynchronously

if summary.Critical.HasFailures then
    failwith "The validation package reported a critical failure."

let outputDirectory = Directory.CreateDirectory(arguments.OutputDirectory)
let combined = RunSummary.combine [| summary.Critical; summary.NonCritical |]

File.WriteAllText(
    Path.Combine(outputDirectory.FullName, "validation_summary.json"),
    ValidationSummary.toJson(summary)
)

File.WriteAllText(
    Path.Combine(outputDirectory.FullName, "validation_report.xml"),
    ARCExpect.JUnit.Writer.toXml(
        combined,
        SuiteName = metadata.Name,
        ?SourceBranch = summary.SourceBranch,
        ?SourceCommitHash = summary.SourceCommitHash
    )
)

File.WriteAllText(
    Path.Combine(outputDirectory.FullName, "badge.svg"),
    ARCExpect.Badge.Writer.toSvg(summary, metadata.Name)
)

printfn $"Passed {summary.Critical.Passed}/{summary.Critical.Total} critical tests."
