module CLITests.PackageCommand

open Expecto
open Expecto.Tests
open Expecto.Impl
open Expecto.Logging
open System.IO
open System.Diagnostics
open Fake.Core
open TestUtils
open ReferenceObjects

open JUnit

let runTool (tool: string) (args: string []) =
    CreateProcess.fromRawCommand tool args
    |> CreateProcess.redirectOutput
    |> Proc.run


[<Tests>]
let ``PackageCommand CLI Tests`` =
    testList "arc validate CLI tests" [
        testSequenced (
            testList "arc-validate package CLI tests" [
                testSequenced (
                    test "arc-validate package install test" {

                        resetConfigEnvironment() // make sure to remove any caching before running these tests
                        let proc = runTool "arc-validate" [|"package"; "install"; "test"|]

                        let package = 
                            Path.Combine(
                                expected_package_cache_folder_path,
                                "test.fsx"
                            )

                        Expect.equal proc.ExitCode 0 $"incorrect exit code.{System.Environment.NewLine}{proc.Result.Output}"
                        Expect.isTrue (Directory.Exists(expected_package_cache_folder_path)) "package cache folder was not created"
                        Expect.isTrue (File.Exists(expected_package_cache_file_path)) "package cache was not created at expected location"
                        Expect.isTrue (File.Exists(package)) "package file was not installed at expected location"
                    }
                )
                testSequenced (
                    test "arc-validate package uninstall test" {
                        let package = 
                            Path.Combine(
                                expected_package_cache_folder_path,
                                "test.fsx"
                            )

                        Expect.isTrue (File.Exists(package)) "package file was not installed at expected location"

                        let proc = runTool "arc-validate" [|"package"; "uninstall"; "test"|]

                        Expect.equal proc.ExitCode 0 $"incorrect exit code.{System.Environment.NewLine}{proc.Result.Output}"
                        Expect.isFalse (File.Exists(package)) "package file was not deleted"
                
                    }
                )
            ]
        )
    ]