module ScriptExecutionTests

open ARCValidationPackages
open Expecto
open System.IO

open ReferenceObjects

[<Tests>]
let ``ScriptExecution tests`` =
    testList "ScriptExecution tests" [
        testList "script paths" [
            test "can execute fixture script without errors" {
                Expect.equal
                    (ScriptExecution.runScript testScriptPath).ExitCode
                    0
                    "script execution did not run without errors."
            }

            test "fixture script prints message" {
                Expect.sequenceEqual
                    (ScriptExecution.runScript testScriptPath).Messages
                    ["Hello, World!"]
                    "script execution did not print correct mesages."
            }

            test "can execute fixture script with args without errors" {
                Expect.equal
                    (ScriptExecution.runScriptWithArgs testScriptArgsPath [|"hello"; "world"|]).ExitCode
                    0
                    "script execution did not run without errors."
            }

            test "fixture script with args prints message" {
                Expect.sequenceEqual
                    (ScriptExecution.runScriptWithArgs testScriptArgsPath [|"hello"; "world"|]).Messages
                    ["""args: [|"hello"; "world"|]"""]
                    "script execution did not print correct mesages."
            }
        ]
        testList "ARCValidationpackages" [
            test "can execute script from package without errors" {
                Expect.equal
                    (ScriptExecution.runPackageScript testScriptPackage).ExitCode
                    0
                    "script execution did not run without errors."
            }

            test "script from package prints message" {
                Expect.sequenceEqual
                    (ScriptExecution.runPackageScript testScriptPackage).Messages
                    ["Hello, World!"]
                    "script execution did not print correct mesages."
            }

            test "can execute script from package with args without errors" {
                Expect.equal
                    (ScriptExecution.runPackageScriptWithArgs testScriptArgsPackage [|"hello"; "world"|]).ExitCode
                    0
                    "script execution did not run without errors."
            }

            test "script from package with args prints message" {
                Expect.sequenceEqual
                    (ScriptExecution.runPackageScriptWithArgs testScriptArgsPackage [|"hello"; "world"|]).Messages
                    ["""args: [|"hello"; "world"|]"""]
                    "script execution did not print correct mesages."
            }
        ]

    ]