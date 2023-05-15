module ARCValidate

open Expecto
open Argu
open System.IO
open ArcValidation
open ArcValidation.Configs

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

        let criticalTests =
            testList "Critical" [
                testList "ARC" [
                    TestGeneration.Critical.Arc.FileSystem.generateArcFileSystemTests arcConfig      // TestGeneration.Critical.Arc.FileSystem...
                ]
                testList "ISA" [
                    TestGeneration.Critical.Arc.ISA.generateISATests arcConfig
                ]
            ]

        let criticalTestResults =
            criticalTests
            |> performTest

        // if critical tests do NOT fail in any way then
        if criticalTestResults.failed |> List.isEmpty && criticalTestResults.errored |> List.isEmpty then
            let nonCriticalTests =
                testList "Non-critical" [
                ]
            let nonCriticalTestResults =
                nonCriticalTests
                |> performTest
            combineTestRunSummaries [criticalTestResults; nonCriticalTestResults]
            |> writeJUnitSummary (Path.Combine(outPath, "arc-validate-results.xml"))
            0
        else
            criticalTestResults
            |> Expecto.writeJUnitSummary (Path.Combine(outPath, "arc-validate-results.xml"))
            1

    with
        | :? ArguParseException as ex ->
            match ex.ErrorCode with
            | ErrorCode.HelpText  -> 
                printfn "%s" (CLIArgs.cliArgParser.PrintUsage())
                0
            | _ -> 
                printfn "%s" ex.Message
                1
        | ex ->
            printfn "Internal Error:"
            printfn "%s" ex.Message
            printfn "%A" ex.StackTrace 
            2
