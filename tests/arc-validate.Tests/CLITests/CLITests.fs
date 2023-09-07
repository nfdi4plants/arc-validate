module asd

//open Expecto
//open Expecto.Tests
//open Expecto.Impl
//open Expecto.Logging
//open System.IO
//open System.Diagnostics
//open Fake.Core

//open JUnit

//let runTool (tool: string) (args: string []) =
//    CreateProcess.fromRawCommand tool args
//    |> CreateProcess.redirectOutput
//    |> Proc.run


//if Directory.Exists(ARCValidationPackages.Defaults.CONFIG_FOLDER()) then Directory.Delete(ARCValidationPackages.Defaults.CONFIG_FOLDER(), true)
//[<Tests>]
//let ``CLI Tests`` =
//    testSequenced (
//        testList "CLI tests" [
//            testSequenced (
//                testList "arc-validate validate -i fixtures/arcs/inveniotestarc" [
//                    let proc = runTool "arc-validate" [|"validate"; "-i"; "fixtures/arcs/inveniotestarc"|] 

//                    test "exit code is 0 (Success)" {
//                        Expect.equal proc.ExitCode 0 "incorrect exit code"
//                    }

//                    test "resultFileExists" {
//                        Expect.isTrue (File.Exists("fixtures/arcs/inveniotestarc/arc-validate-results.xml")) "result file does not exist at expected location"
//                    }

//                    let validationResults = ValidationResults.fromJUnitFile "fixtures/arcs/inveniotestarc/arc-validate-results.xml"

//                    test "passed tests"{
//                        Expect.equal  
//                            validationResults.PassedTests
//                            ReferenceObjects.``invenio test arc validation results``.PassedTests
//                            "incorrect test results"
            
//                    }
//                    test "failed tests" {
            
//                        Expect.equal  
//                            validationResults.FailedTests
//                            ReferenceObjects.``invenio test arc validation results``.FailedTests
//                            "incorrect test results"
//                    }
//                    test "errored tests" {
//                        Expect.equal  
//                            validationResults.ErroredTests
//                            ReferenceObjects.``invenio test arc validation results``.ErroredTests
//                            "incorrect test results"
//                    }
//                ]
//            )
//            testSequenced (
//                testList "arc-validate package install test" [
//                    let proc = runTool "arc-validate" [|"package"; "install"; "test"|]
//                    test "exit code is 0 (Success)" {
//                        Expect.equal proc.ExitCode 0 $"incorrect exit code.{System.Environment.NewLine}{proc.Result.Output}"
//                    }

//                    test "package cache folder is created" {
//                        Expect.isTrue (Directory.Exists(ARCValidationPackages.Defaults.PACKAGE_CACHE_FOLDER())) "package cache folder was not created"
//                    }

//                    test "package cache file is created" {
//                        Expect.isTrue (File.Exists(ARCValidationPackages.Defaults.PACKAGE_CACHE_FILE_PATH())) "package cache was not created at expected location"
//                    }

//                    test "installed package file exists" {
//                        let package = 
//                            Path.Combine(
//                                ARCValidationPackages.Defaults.PACKAGE_CACHE_FOLDER(),
//                                "test.fsx"
//                            )
//                        Expect.isTrue (File.Exists(package)) "package file was not installed at expected location"
//                    }
//                ]
//            )
//            testSequenced (
//                ptestList "arc-validate package uninstall test" [
//                    test "installed package file exists before deletion" {
//                        let package = 
//                            Path.Combine(
//                                ARCValidationPackages.Defaults.PACKAGE_CACHE_FOLDER(),
//                                "test.fsx"
//                            )
//                        Expect.isTrue (File.Exists(package)) "package file was not installed at expected location"
//                    }
//                    let proc = runTool "arc-validate" [|"package"; "uninstall"; "test"|]
//                    test "exit code is 0 (Success)" {
//                        Expect.equal proc.ExitCode 0 $"incorrect exit code.{System.Environment.NewLine}{proc.Result.Output}"
//                    }
//                    test "installed package file is deleted" {
//                        let package = 
//                            Path.Combine(
//                                ARCValidationPackages.Defaults.PACKAGE_CACHE_FOLDER(),
//                                "test.fsx"
//                            )
//                        Expect.isFalse (File.Exists(package)) "package file was not deleted"
//                    }
//                ]
//            )
//        ]
//    )