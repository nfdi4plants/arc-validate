module CLITests.ValidateCommand

open Expecto
open Expecto.Tests
open Expecto.Impl
open Expecto.Logging
open System.IO
open System.Diagnostics
open Fake.Core

open ARCExpect

open Common
open Common.TestUtils
open TestUtils

open JUnit

[<Tests>]
let ``ValidateCommand CLI Tests`` =
    testSequenced (testList "arc-validate validate" [
        testSequenced (testList "specification validation" [
            testSequenced (testList "latest" [
                // run: arc-validate validate -i fixtures/arcs/specification/v2.0.0-draft --source-branch refs/heads/dev --source-commit-hash 0123456789abcdef
                // adapt this when a new latest specification package is available!
                yield!
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|
                            "--verbose"
                            "validate"
                            "-i"
                            "fixtures/arcs/specification/v2.0.0-draft"
                            "-o"
                            "."
                            "--source-branch"
                            "refs/heads/dev"
                            "--source-commit-hash"
                            "0123456789abcdef"
                        |]
                        
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Console Output indicates that the tool will validate against specs" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("running `arc-validate validate` without")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Console Output indicates that the chosen spec version is latest" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("Performing validation against version 'latest' of the ARC specification")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Console Output indicates that latest spec version is mapped to a validation package correctly" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("latest spec version supported is 2.0.0-draft")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Ouptput files exist",
                            fun tool args proc -> 
                                Expect.isTrue (Directory.Exists(".arc-validate-results")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/badge.svg")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/badge.svg does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/validation_report.xml")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/validation_report.xml does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                        "Test arc passes spec validation",
                            fun tool args proc -> 
                                let summary = 
                                    ".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json"
                                    |> File.ReadAllText
                                    |> fun x -> x.ReplaceLineEndings("\n")
                                    |> ValidationSummary.fromJson

                                Expect.equal summary.Critical.Failed 0 (ErrorMessage.withProcessDiagnostics "incorrect number of critical failures" proc tool args )
                                Expect.equal summary.Critical.Errored 0 (ErrorMessage.withProcessDiagnostics "incorrect number of critical errors" proc tool args )
                                Expect.isFalse summary.Critical.HasFailures (ErrorMessage.withProcessDiagnostics "expected no critical failures" proc tool args )
                                
                                Expect.equal summary.Critical.Failed 0 (ErrorMessage.withProcessDiagnostics "incorrect number of noncritical failures" proc tool args )
                                Expect.equal summary.Critical.Errored 0 (ErrorMessage.withProcessDiagnostics "incorrect number of noncritical failures" proc tool args )
                                Expect.isFalse summary.Critical.HasFailures (ErrorMessage.withProcessDiagnostics "expected no noncritical failures" proc tool args )

                                Expect.equal summary.SourceBranch (Some "refs/heads/dev") (ErrorMessage.withProcessDiagnostics "incorrect source branch" proc tool args )
                                Expect.equal summary.SourceCommitHash (Some "0123456789abcdef") (ErrorMessage.withProcessDiagnostics "incorrect source commit hash" proc tool args )

                                let report = File.ReadAllText ".arc-validate-results/arc_specification@2.0.0-draft/validation_report.xml"
                                let badge = File.ReadAllText ".arc-validate-results/arc_specification@2.0.0-draft/badge.svg"
                                Expect.stringContains report "name=\"SourceBranch\" value=\"refs/heads/dev\"" (ErrorMessage.withProcessDiagnostics "JUnit source branch missing" proc tool args )
                                Expect.stringContains report "name=\"SourceCommitHash\" value=\"0123456789abcdef\"" (ErrorMessage.withProcessDiagnostics "JUnit source commit missing" proc tool args )
                                Expect.stringContains badge "SourceBranch=\"refs/heads/dev\"" (ErrorMessage.withProcessDiagnostics "badge source branch missing" proc tool args )
                                Expect.stringContains badge "SourceCommitHash=\"0123456789abcdef\"" (ErrorMessage.withProcessDiagnostics "badge source commit missing" proc tool args )

                    ]
            ])
            testSequenced (testList "v2-0-0-draft" [
                // run: arc-validate validate --specification-version 2.0.0-draft -i fixtures/arcs/specification/v2.0.0-draft
                yield! 
                    testFixture (Fixtures.withToolExecution 
                        false
                        "../../../../../publish/arc-validate" 
                        [|"--verbose"; "validate"; "--specification-version"; "2.0.0-draft"; "-i"; "fixtures/arcs/specification/v2.0.0-draft"; "-o"; "."|]
                        
                    ) [
                        "Exit code is 0" , 
                            fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                        "Console Output indicates that the tool will validate against specs" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("running `arc-validate validate` without")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Console Output indicates that the chosen spec version is correct" ,
                            fun tool args proc -> Expect.isTrue (proc.Result.Output.Contains("Performing validation against version '2.0.0-draft' of the ARC specification")) (ErrorMessage.withProcessDiagnostics "incorrect console output" proc tool args )
                        "Ouptput files exist",
                            fun tool args proc -> 
                                Expect.isTrue (Directory.Exists(".arc-validate-results")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/badge.svg")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/badge.svg does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/validation_report.xml")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/validation_report.xml does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                                Expect.isTrue (File.Exists(".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json")) (ErrorMessage.withProcessDiagnostics $".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json does not exist in {System.Environment.CurrentDirectory}" proc tool args )
                        "Test arc passes spec validation",
                            fun tool args proc -> 
                                let summary = 
                                    ".arc-validate-results/arc_specification@2.0.0-draft/validation_summary.json"
                                    |> File.ReadAllText
                                    |> fun x -> x.ReplaceLineEndings("\n")
                                    |> ValidationSummary.fromJson

                                Expect.equal summary.Critical.Failed 0 (ErrorMessage.withProcessDiagnostics "incorrect number of critical failures" proc tool args )
                                Expect.equal summary.Critical.Errored 0 (ErrorMessage.withProcessDiagnostics "incorrect number of critical errors" proc tool args )
                                Expect.isFalse summary.Critical.HasFailures (ErrorMessage.withProcessDiagnostics "expected no critical failures" proc tool args )
                                
                                Expect.equal summary.Critical.Failed 0 (ErrorMessage.withProcessDiagnostics "incorrect number of noncritical failures" proc tool args )
                                Expect.equal summary.Critical.Errored 0 (ErrorMessage.withProcessDiagnostics "incorrect number of noncritical failures" proc tool args )
                                Expect.isFalse summary.Critical.HasFailures (ErrorMessage.withProcessDiagnostics "expected no noncritical failures" proc tool args )

                    ]
            ])
        ])
    ])
