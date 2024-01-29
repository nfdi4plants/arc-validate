module CLITests.PackageCommand

open Expecto
open Expecto.Tests
open Expecto.Impl
open Expecto.Logging
open System.IO
open System.Diagnostics
open Fake.Core
open Common.TestUtils
open TestUtils
open ReferenceObjects

open JUnit

[<Tests>]
let ``PackageCommand CLI Tests`` =
    testSequenced (testList "arc-validate package" [
        testSequenced (testList "install" [
            yield! 
                testFixture (Fixtures.withToolExecution 
                    "../../../../../publish/arc-validate" 
                    [|"-v"; "package"; "install"; "test"|]
                    (get_gh_api_token())
                ) [
                    "Exit code is 0" , 
                        fun tool args proc -> Expect.equal proc.ExitCode 0 $"""incorrect exit code.{System.Environment.NewLine}{proc.Result.Output} (tool: {tool} args: {args |> String.concat " "})"""
                    "Cache folder exists" ,  
                        fun tool args proc -> Expect.isTrue (Directory.Exists(expected_package_cache_folder_path)) $"""package cache folder was not created at {expected_package_cache_folder_path} (tool: {tool} args: {args |> String.concat " "})"""
                    "Cache exists" ,  
                        fun tool args proc -> Expect.isTrue (File.Exists(expected_package_cache_file_path)) $"""package cache was not created at {expected_package_cache_file_path} (tool: {tool} args: {args |> String.concat " "})"""
                    "Package script exists" ,  
                        fun tool args proc -> Expect.isTrue (File.Exists(Path.Combine(expected_package_cache_folder_path, "test.fsx"))) $"""package file was not installed at expected location (tool: {tool} args: {args |> String.concat " "})"""
                ]
        ])

        testSequenced (testList "update" [])
        testSequenced (testList "delete" [])
        testSequenced (testList "update-index" [])
    ])