module CLITests

open Expecto
open Expecto.Tests
open Expecto.Impl
open Expecto.Logging
open System.IO
open System.Diagnostics

open JUnit

let runTool (tool: string) (args: string) =
    let procStartInfo = 
        ProcessStartInfo(
            UseShellExecute = false,
            FileName = tool,
            Arguments = args
        )
    let proc = Process.Start(procStartInfo)
    proc.WaitForExit()
    proc

if Directory.Exists(ARCValidationPackages.Defaults.CONFIG_FOLDER()) then Directory.Delete(ARCValidationPackages.Defaults.CONFIG_FOLDER(), true)

[<Tests>]
let ``CLI Tests`` =
    testList "CLI tests" [
        testList "arc-validate validate -i fixtures/arcs/inveniotestarc" [
            let proc = runTool "arc-validate" "validate -i fixtures/arcs/inveniotestarc" 

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

        testSequenced (
            testList "arc-validate package install test" [
                let proc = runTool "arc-validate" "package install test"
                test "exit code is 0 (Success)" {
                    Expect.equal proc.ExitCode 0 $"incorrect exit code."
                }

                test "package cache folder is created" {
                    Expect.isTrue (Directory.Exists(ARCValidationPackages.Defaults.PACKAGE_CACHE_FOLDER())) "package cache folder wa snot created"
                }

                test "package cache file is created" {
                    Expect.isTrue (File.Exists(ARCValidationPackages.Defaults.PACKAGE_CACHE_FILE_PATH())) "package cache was not created at expected location"
                }

                test "installed package file exists" {
                    let package = 
                        Path.Combine(
                            ARCValidationPackages.Defaults.PACKAGE_CACHE_FOLDER(),
                            "test.fsx"
                        )
                    Expect.isTrue (File.Exists(package)) "package file was not installed at expected location"
                }
            ]
        )
    ]