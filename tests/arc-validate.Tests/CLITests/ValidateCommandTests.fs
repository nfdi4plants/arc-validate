module CLITests.ValidateCommand

open Expecto
open Expecto.Tests
open Expecto.Impl
open Expecto.Logging
open System.IO
open System.Diagnostics
open Fake.Core

open Common
open Common.TestUtils
open TestUtils

open JUnit

[<Tests>]
let ``ValidateCommand CLI Tests`` =
    testSequenced (testList "arc-validate validate" [
        testSequenced (testList "package test -i fixtures/arcs/inveniotestarc" [
            yield! 
                testFixture (Fixtures.withToolExecution 
                    true
                    "../../../../../publish/arc-validate" 
                    [|"-v"; "package"; "install"; "test"|]
                    (get_gh_api_token())
                ) [
                    "Package script exists after running package install test" ,  
                        fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path, "test.fsx")))  (ErrorMessage.withCLIDiagnostics "package file was not installed at expected location" tool args )
                ]
            yield! 
                testFixture (Fixtures.withToolExecution 
                    true
                    "../../../../../publish/arc-validate" 
                    [|"-v"; "validate"; "-p"; "test"; "-i"; "fixtures/arcs/inveniotestarc"|]
                    (get_gh_api_token())
                ) [
                    "Exit code is 0" , 
                        fun tool args proc -> Expect.equal proc.ExitCode 0 (ErrorMessage.withProcessDiagnostics "incorrect exit code" proc tool args )
                    ]
        ])

        //        printfn "%s" (Path.GetFullPath("fixtures/arcs/inveniotestarc"))

        //        let proc = runTool "../../../../../publish/arc-validate" [|"validate"; "-i"; "fixtures/arcs/inveniotestarc"|] 

        //        printfn "%s" proc.Result.Output
        //        printfn "%s" proc.Result.Error

        //        test "exit code is 0 (Success)" {
        //            Expect.equal proc.ExitCode 0 $"incorrect exit code: {proc.Result.Error} | {proc.Result.Output}"
        //        }

        //        test "resultFileExists" {
        //            Expect.isTrue (File.Exists("fixtures/arcs/inveniotestarc/arc-validate-results.xml")) "result file does not exist at expected location"
        //        }

        //        test "passed tests"{
        //            let validationResults = ValidationResults.fromJUnitFile "fixtures/arcs/inveniotestarc/arc-validate-results.xml"
        //            Expect.equal  
        //                validationResults.PassedTests
        //                ReferenceObjects.``invenio test arc validation results``.PassedTests
        //                "incorrect test results"
            
        //        }

        //        test "failed tests" {
        //            let validationResults = ValidationResults.fromJUnitFile "fixtures/arcs/inveniotestarc/arc-validate-results.xml"
        //            Expect.equal  
        //                validationResults.FailedTests
        //                ReferenceObjects.``invenio test arc validation results``.FailedTests
        //                "incorrect test results"
        //        }

        //        test "errored tests" {
        //            let validationResults = ValidationResults.fromJUnitFile "fixtures/arcs/inveniotestarc/arc-validate-results.xml"
        //            Expect.equal  
        //                validationResults.ErroredTests
        //                ReferenceObjects.``invenio test arc validation results``.ErroredTests
        //                "incorrect test results"
        //        }
        //    ]
        //)
    ])