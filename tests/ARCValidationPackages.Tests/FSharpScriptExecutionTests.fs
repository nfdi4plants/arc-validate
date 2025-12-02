module FSharpScriptExecutionTests

open ARCValidationPackages
open Expecto
open System.IO

open ReferenceObjects
open Common.TestUtils
open TestUtils

[<Tests>]
let ``FSharp ScriptExecution tests`` =
    testList "FSharp ScriptExecution tests" [
        testList "script paths" [
            test "can execute fixture script without errors" {
                let result = FSharpScript.run fsharpTestScriptPath
                Expect.sequenceEqual result.Errors [] "script execution did not run without errors."
                Expect.equal result.ExitCode 0 "script execution did not run without errors."
            }

            test "fixture script prints message" {
                Expect.sequenceEqual
                    (FSharpScript.run fsharpTestScriptPath).Messages
                    ["Hello, World!"]
                    "script execution did not print correct mesages."
            }

            test "can execute fixture script with args without errors" {
                let result = FSharpScript.runWithArgs fsharpTestScriptArgsPath [|"hello"; "world"|]
                Expect.sequenceEqual result.Errors [] "script execution did not run without errors."
                Expect.equal result.ExitCode 0 "script execution did not run without errors."
            }

            test "fixture script with args prints message" {
                Expect.sequenceEqual
                    (FSharpScript.runWithArgs fsharpTestScriptArgsPath [|"hello"; "world"|]).Messages
                    ["""args: [|"hello"; "world"|]"""]
                    "script execution did not print correct mesages."
            }
        ]
        testList "ARCValidationpackages" [
            test "can execute script from package without errors" {
                let result = FSharpScript.runPackageScript CachedValidationPackage.fsharpTestScriptPackage
                Expect.sequenceEqual result.Errors [] "script execution did not run without errors."
                Expect.equal result.ExitCode 0 "script execution did not run without errors."
            }

            test "script from package prints message" {
                Expect.sequenceEqual
                    (FSharpScript.runPackageScript CachedValidationPackage.fsharpTestScriptPackage).Messages
                    ["Hello, World!"]
                    "script execution did not print correct mesages."
            }

            test "can execute script from package with args without errors" {
                let result = FSharpScript.runPackageScriptWithArgs CachedValidationPackage.fsharpTestScriptArgsPackage [|"hello"; "world"|]
                Expect.sequenceEqual result.Errors [] "script execution did not run without errors."
                Expect.equal result.ExitCode 0 "script execution did not run without errors."
            }

            test "script from package with args prints message" {
                Expect.sequenceEqual
                    (FSharpScript.runPackageScriptWithArgs CachedValidationPackage.fsharpTestScriptArgsPackage [|"hello"; "world"|]).Messages
                    ["""args: [|"hello"; "world"|]"""]
                    "script execution did not print correct mesages."
            }
        ]

    ]