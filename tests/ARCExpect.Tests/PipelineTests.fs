module PipelineTests

open System
open System.Collections.Generic
open System.IO
open ARCExpect
open Expecto
open ValidationPackage.Model

let private metadata =
    ValidationPackageMetadata.create(
        "pipeline-test",
        "Pipeline test",
        "Verifies the .NET output boundary.",
        1,
        0,
        0,
        "FSharp"
    )

let private package() =
    Setup.ValidationPackage(
        metadata,
        CriticalValidationCases = [|
            Fable.Pyxpecto.Test.testCase "passes" (fun () -> ())
        |]
    )

[<Tests>]
let tests =
    testList "portable validation pipeline" [
        test "writes all standard outputs without exposing writer plumbing" {
            let basePath =
                Path.Combine(
                    Path.GetTempPath(),
                    $"arcexpect-pipeline-{Guid.NewGuid():N}"
                )

            try
                package()
                |> Execute.ValidationPipeline(
                    basePath,
                    SourceBranch = "feature/pipeline",
                    SourceCommitHash = "abc123"
                )

                let resultFolder =
                    Path.Combine(
                        basePath,
                        ".arc-validate-results",
                        "pipeline-test@1.0.0"
                    )

                let summaryPath =
                    Path.Combine(resultFolder, "validation_summary.json")
                let junitPath =
                    Path.Combine(resultFolder, "validation_report.xml")
                let badgePath = Path.Combine(resultFolder, "badge.svg")

                Expect.isTrue (File.Exists summaryPath) "summary exists"
                Expect.isTrue (File.Exists junitPath) "JUnit exists"
                Expect.isTrue (File.Exists badgePath) "badge exists"

                let summary =
                    File.ReadAllText(summaryPath)
                    |> ValidationSummary.fromJson

                Expect.equal summary.Critical.Passed 1 "critical result"
                Expect.equal summary.SourceBranch (Some "feature/pipeline") "source branch"
                Expect.equal summary.SourceCommitHash (Some "abc123") "source commit"

                let junit = File.ReadAllText(junitPath)
                let badge = File.ReadAllText(badgePath)
                Expect.stringContains junit "[ Critical; passes ]" "combined run summary"
                Expect.stringContains junit "SourceBranch" "JUnit provenance"
                Expect.stringContains badge "SourceBranch=\"feature/pipeline\"" "badge provenance"
            finally
                if Directory.Exists basePath then
                    Directory.Delete(basePath, true)
        }

        test "accepts native .NET dictionaries as payloads" {
            let basePath =
                Path.Combine(
                    Path.GetTempPath(),
                    $"arcexpect-payload-{Guid.NewGuid():N}"
                )

            let payload =
                dict [
                    "Metrics", box (dict [ ("FilesChecked", box 2) ])
                    "Label", box "pipeline"
                ]

            try
                package()
                |> Execute.ValidationPipeline(basePath, Payload = payload)

                let summaryPath =
                    Path.Combine(
                        basePath,
                        ".arc-validate-results",
                        "pipeline-test@1.0.0",
                        "validation_summary.json"
                    )

                let summaryJson = File.ReadAllText(summaryPath)
                Expect.stringContains summaryJson "\"FilesChecked\":2" "nested number"
                Expect.stringContains summaryJson "\"Label\":\"pipeline\"" "string"
            finally
                if Directory.Exists basePath then
                    Directory.Delete(basePath, true)
        }

        test "package arguments select the output directory" {
            let basePath =
                Path.Combine(
                    Path.GetTempPath(),
                    $"arcexpect-arguments-{Guid.NewGuid():N}"
                )

            let arguments =
                PackageArguments.parse(
                    metadata,
                    [|
                        "-i"
                        "arc"
                        "-o"
                        basePath
                        "--source-branch"
                        "feature/arguments"
                    |]
                )

            try
                package() |> Execute.ValidationPipeline(arguments)

                let summaryPath =
                    Path.Combine(
                        basePath,
                        ".arc-validate-results",
                        "pipeline-test@1.0.0",
                        "validation_summary.json"
                    )

                let summary =
                    File.ReadAllText(summaryPath)
                    |> ValidationSummary.fromJson

                Expect.equal summary.SourceBranch (Some "feature/arguments") "argument provenance"
            finally
                if Directory.Exists basePath then
                    Directory.Delete(basePath, true)
        }

        test "individual output methods remain available" {
            let basePath =
                Path.Combine(
                    Path.GetTempPath(),
                    $"arcexpect-individual-outputs-{Guid.NewGuid():N}"
                )

            Directory.CreateDirectory(basePath) |> ignore

            let summary =
                Execute.Validation(package())
                |> Async.RunSynchronously

            let summaryPath = Path.Combine(basePath, "validation_summary.json")
            let junitPath = Path.Combine(basePath, "validation_report.xml")
            let badgePath = Path.Combine(basePath, "badge.svg")

            try
                summary |> Execute.SummaryCreation(summaryPath)
                summary |> Execute.JUnitReportCreation(junitPath)
                summary |> Execute.BadgeCreation(badgePath, "custom badge")

                Expect.isTrue (File.Exists summaryPath) "summary exists"
                Expect.isTrue (File.Exists junitPath) "JUnit exists"
                Expect.isTrue (File.Exists badgePath) "badge exists"
                Expect.stringContains
                    (File.ReadAllText badgePath)
                    "custom badge"
                    "custom badge label"
            finally
                if Directory.Exists basePath then
                    Directory.Delete(basePath, true)
        }
    ]
