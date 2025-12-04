module PythonScriptExecutionTests

open ARCValidationPackages
open Expecto
open System.IO

open ReferenceObjects
open Common.TestUtils
open TestUtils

[<Tests>]
let ``Python ScriptExecution tests`` =
    testList "Python ScriptExecution tests" [
        testList "script paths" [
            test "can execute fixture script without errors" {
                let result = PythonScript.run pythonTestScriptPath
                //Expect.sequenceEqual result.Errors [] "script execution did not run without errors."
                Expect.equal result.ExitCode 0 "script execution did not run without errors."
            }

            test "fixture script prints message" {
                Expect.sequenceEqual
                    (PythonScript.run pythonTestScriptPath).Messages
                    ["Hello, World!"]
                    "script execution did not print correct mesages."
            }

            test "can execute fixture script with args without errors" {
                Expect.equal
                    (PythonScript.runWithArgs pythonTestScriptArgsPath [|"hello"; "world"|]).ExitCode
                    0
                    "script execution did not run without errors."
            }

            test "fixture script with args prints message" {
                Expect.sequenceEqual
                    (PythonScript.runWithArgs pythonTestScriptArgsPath [|"hello"; "world"|]).Messages
                    ["""args: ['hello', 'world']"""]
                    "script execution did not print correct mesages."
            }
        ]
        testList "ARCValidationpackages" [
            test "can execute script from package without errors" {
                let result = PythonScript.runPackageScript CachedValidationPackage.pythonTestScriptPackage
                //Expect.sequenceEqual result.Errors [] "script execution did not run without errors."
                Expect.equal result.ExitCode 0 "script execution did not run without errors."
            }

            test "script from package prints message" {
                Expect.sequenceEqual
                    (PythonScript.runPackageScript CachedValidationPackage.pythonTestScriptPackage).Messages
                    ["Hello, World!"]
                    "script execution did not print correct mesages."
            }

            test "can execute script from package with args without errors" {
                let result = PythonScript.runPackageScriptWithArgs CachedValidationPackage.pythonTestScriptArgsPackage [|"hello"; "world"|]
                Expect.sequenceEqual result.Errors [] "script execution did not run without errors."
                Expect.equal result.ExitCode 0 "script execution did not run without errors."
            }

            test "script from package with args prints message" {
                Expect.sequenceEqual
                    (PythonScript.runPackageScriptWithArgs CachedValidationPackage.pythonTestScriptArgsPackage [|"hello"; "world"|]).Messages
                    ["""args: ['hello', 'world']"""]
                    "script execution did not print correct mesages."
            }
        ]

    ]