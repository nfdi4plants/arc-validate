let [<Literal>]PACKAGE_METADATA = """(*
---
Name: configurable-validation
Summary: A validation package with typed inputs
Description: Reads standard and CWL-defined arguments through ARCExpect.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
Inputs:
  - id: strict
    type: boolean
    inputBinding:
      prefix: --strict
      position: 0
      separate: true
  - id: minimum-files
    type: int
    inputBinding:
      prefix: --minimum-files
      position: 0
      separate: true
  - id: label
    type: string?
    inputBinding:
      prefix: --label
      position: 0
      separate: true
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
let minimumFiles = arguments.GetInt("minimum-files")

let validationPackage =
    Setup.ValidationPackage(
        metadata,
        CriticalValidationCases = [|
            testCase "minimum-files is positive" <| fun () ->
                Expect.isTrue (minimumFiles > 0) "minimum-files"

            testCase "strict mode has a label" <| fun () ->
                if arguments.GetBoolean("strict") then
                    Expect.isTrue (arguments.TryGetString("label").IsSome) "label"
        |]
    )

let summary =
    Execute.Validation(validationPackage, Arguments = arguments)
    |> Async.RunSynchronously

if summary.Critical.HasFailures then
    failwith "The configurable validation package failed."

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

printfn $"Validated {arguments.ArcDirectory}; results belong in {arguments.OutputDirectory}."
