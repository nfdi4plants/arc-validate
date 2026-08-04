module PythonScriptExecutionTests

open ARCValidate.PackageManagement
open ARCValidate.PackageRunner
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

            test "hostile-looking arguments remain literal process values" {
                let hostileValue = "\"; print('INJECTED'); $(touch injected) & <xml>"
                let result = PythonScript.runWithArgs pythonTestScriptArgsPath [| hostileValue |]

                Expect.equal result.ExitCode 0 "Literal argument execution failed."
                Expect.equal result.Messages.Length 1 "The value must not create another command or output line."
                Expect.stringContains result.Messages.Head "print(\\'INJECTED\\')" "Python repr preserves the quoted text."
                Expect.stringContains result.Messages.Head "$(touch injected)" "Shell syntax must remain literal."
                Expect.stringContains result.Messages.Head "& <xml>" "Shell and markup characters must remain literal."
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
