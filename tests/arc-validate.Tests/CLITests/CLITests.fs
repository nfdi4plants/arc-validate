module CLITests

open Expecto
open Expecto.Tests
open Expecto.Impl
open Expecto.Logging
open System.IO
open System.Diagnostics

open JUnit

type CLIResult = {
    Process: Process
    ValidationResults: ValidationResults
}

type CLIContext() =
    static member create 
        (
            ?i: string,
            ?o : string
        ) = 
            fun f ->
                let args = 
                    [
                        "validate "
                        i |> Option.map (fun p -> $"-i {i} ") |> Option.defaultValue ""
                        o |> Option.map (fun o -> $"-o {o} ") |> Option.defaultValue ""
                    ]
                    |> String.concat ""

                let outFile = 
                    o
                    |> Option.defaultValue (System.Environment.GetEnvironmentVariable("ARC_PATH")) // default to ARC_PATH if argument is not provided
                    |> fun s -> 
                        if i.IsSome then 
                            i.Value else 
                        if System.String.IsNullOrWhiteSpace(s) then 
                            System.Environment.CurrentDirectory 
                        else s // default to ./ if ARC_PATH is not set
                    |> fun s -> Path.Combine(s, "arc-validate-results.xml")

                let procStartInfo = 
                    ProcessStartInfo(
                        UseShellExecute = false,
                        FileName = "arc-validate",
                        Arguments = args
                    )
                printfn "%A" System.Environment.CurrentDirectory
                printfn "%A" procStartInfo

                let proc = Process.Start(procStartInfo)
                proc.WaitForExit()
                let result = ValidationResults.fromJUnitFile outFile
                  
                {
                    Process = proc
                    ValidationResults = result
                }
                |> f

[<Tests>]
let ``CLI Tests`` =
    testList "CLI tests" [
        testList "fixtures/arcs/inveniotestarc" [
            yield! testFixture (CLIContext.create(i = "fixtures/arcs/inveniotestarc")) [
                "exit code is 0 (Success)", (fun testResults -> fun () -> 
                    Expect.equal  
                        testResults.Process.ExitCode
                        0
                        "incorrect exit code"
                )
                "total test amount", (fun testResults -> fun () -> 
                    Expect.equal  
                        (ValidationResults.getTotalTestCount testResults.ValidationResults)
                        (ValidationResults.getTotalTestCount ReferenceObjects.``invenio test arc validation results``)
                        "incorrect total amount of tests"
                )
                "passed tests", (fun testResults -> fun () -> 
                    Expect.equal  
                        testResults.ValidationResults.PassedTests
                        ReferenceObjects.``invenio test arc validation results``.PassedTests
                        "incorrect test results"
                )
                "failed tests", (fun testResults -> fun () -> 
                    Expect.equal  
                        testResults.ValidationResults.FailedTests
                        ReferenceObjects.``invenio test arc validation results``.FailedTests
                        "incorrect test results"
                )
                "errored tests", (fun testResults -> fun () -> 
                    Expect.equal  
                        testResults.ValidationResults.ErroredTests
                        ReferenceObjects.``invenio test arc validation results``.ErroredTests
                        "incorrect test results"
                )
            ]
        ]
    ]