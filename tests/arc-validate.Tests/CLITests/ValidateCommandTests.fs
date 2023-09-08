module CLITests.ValidateCommand

open Expecto
open Expecto.Tests
open Expecto.Impl
open Expecto.Logging
open System.IO
open System.Diagnostics
open Fake.Core

open JUnit

let runTool (tool: string) (args: string []) =
    CreateProcess.fromRawCommand tool args
    |> CreateProcess.redirectOutput
    |> Proc.run

[<Tests>]
let ``ValidateCommand CLI Tests`` =
    testList "arc validate CLI tests" [
        testSequenced (
            testList "arc-validate validate -i fixtures/arcs/inveniotestarc" [
                let proc = runTool "arc-validate" [|"validate"; "-i"; "fixtures/arcs/inveniotestarc"|] 

                test "exit code is 0 (Success)" {
                    Expect.equal proc.ExitCode 0 "incorrect exit code"
                }

                test "resultFileExists" {
                    Expect.isTrue (File.Exists("fixtures/arcs/inveniotestarc/arc-validate-results.xml")) "result file does not exist at expected location"
                }

                let validationResults = ValidationResults.fromJUnitFile "fixtures/arcs/inveniotestarc/arc-validate-results.xml"

                test "passed tests"{
                    Expect.equal  
                        validationResults.PassedTests
                        ReferenceObjects.``invenio test arc validation results``.PassedTests
                        "incorrect test results"
            
                }
                test "failed tests" {
            
                    Expect.equal  
                        validationResults.FailedTests
                        ReferenceObjects.``invenio test arc validation results``.FailedTests
                        "incorrect test results"
                }
                test "errored tests" {
                    Expect.equal  
                        validationResults.ErroredTests
                        ReferenceObjects.``invenio test arc validation results``.ErroredTests
                        "incorrect test results"
                }
            ]
        )
    ]