module ARCValidate

open Expecto
open Argu
open System.IO
open ArcValidation
open ArcValidation.Configs
open ExitCodes

    
let createInternalFailDummyTestResults () = 
    testList "arc-validate" [
        testCase "arc-validate" (fun _ -> 
            Expect.isFalse true """arc-validate failed due to an internal error
This error did likely NOT occur due to user input.
An empty test result file will be created to reflect this and prevent the subsequent validation pipeline from failing.
"""
        )
    ]
    |> performTest


[<EntryPoint>]
let main argv =
    try
        let args = CLIArgs.cliArgParser.ParseCommandLine()
        let arcConfig = 
            args.TryGetResult(CLIArgs.ARC_Directory)
            |> Option.defaultValue (System.Environment.GetEnvironmentVariable("ARC_PATH")) // default to ARC_PATH if argument is not provided
            |> fun s -> if System.String.IsNullOrWhiteSpace(s) then System.Environment.CurrentDirectory else s // default to ./ if ARC_PATH is not set
            |> fun s -> ArcConfig(s)

        let outPath = 
            args.TryGetResult(CLIArgs.Out_Directory)
            |> Option.defaultValue arcConfig.PathConfig.ArcRootPath

        let verbose = args.TryGetResult(CLIArgs.Verbose).IsSome
        try
            /// these tests MUST pass for an ARC to be considered for publishing
            let criticalTests =
                testList "Critical" [
                    TestGeneration.Critical.Arc.FileSystem.generateArcFileSystemTests arcConfig
                    TestGeneration.Critical.Arc.ISA.generateISATests arcConfig
                ]

            let criticalTestResults =
                criticalTests
                |> performTest

            if criticalTestResults.failed |> List.isEmpty && criticalTestResults.errored |> List.isEmpty then // if no critical tests failed or errored
            
                /// these tests SHOULD pass for an ARC to be considered of high quality
                let nonCriticalTests =
                    testList "Non-critical" [
                        TestGeneration.NonCritical.Arc.ISA.generateISATests arcConfig
                    ]

                let nonCriticalTestResults =
                    nonCriticalTests
                    |> performTest

                [criticalTestResults; nonCriticalTestResults] 
                |> combineTestRunSummaries // aggregate critical and non-critical test results
                |> writeJUnitSummary verbose (Path.Combine(outPath, "arc-validate-results.xml")) // write the combined result to a single file

                ExitCode.Success |> int // critical tests passed, non-critical tests have been performed. Success!

            else // one or more critical tests failed or errored.
                criticalTestResults
                |> Expecto.writeJUnitSummary verbose (Path.Combine(outPath, "arc-validate-results.xml"))

                ExitCode.CriticalTestFailure |> int
        with
            | ex -> 
                printfn "arc-validate failed due to an internal error."
                printfn "This error did likely NOT occur due to user input."
                printfn "An empty test result file will be created to reflect this and prevent the validation pipeline from failing."
                
                if verbose then
                    printfn "%s" ex.Message
                    printfn "%A" ex.StackTrace // might want to add verbosity level to hide this
                else
                    printfn "Run arc-validate with --verbose to see the full error message."

                createInternalFailDummyTestResults()
                |> Expecto.writeJUnitSummary false (Path.Combine(outPath, "arc-validate-results.xml"))

                ExitCode.InternalError |> int
    with
        | :? ArguParseException as ex ->
            match ex.ErrorCode with
            | ErrorCode.HelpText  -> 
                printfn "%s" (CLIArgs.cliArgParser.PrintUsage())
                ExitCode.Success |> int // printing usage is not an error

            | ErrorCode.CommandLine ->
                printfn "Argument parsing error:"
                printfn "%s" ex.Message
                printfn "%A" ex.StackTrace // might want to add verbosity level to hide this
                ExitCode.ArgParseError |> int

            | _ -> 
                printfn "arc-validate failed due to an internal error."
                printfn "This error did likely NOT occur due to user input."
                printfn "An empty test result file will be created to reflect this and prevent the validation pipeline from failing."
                printfn "%s" ex.Message
                printfn "%A" ex.StackTrace // might want to add verbosity level to hide this

                createInternalFailDummyTestResults()
                |> Expecto.writeJUnitSummary false ("arc-validate-results.xml")

                ExitCode.InternalError |> int
        | ex ->
            printfn "Internal Error:"
            printfn "%s" ex.Message
            printfn "%A" ex.StackTrace // might want to add verbosity level to hide this

            ExitCode.InternalError |> int
            