module ARCValidate

open Expecto
open Argu
open System.IO
open ArcValidation
open ArcValidation.Configs

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

    printfn "arc root path: %s" arcConfig.PathConfig.ArcRootPath
    printfn "outpath: %s" outPath

    testList "ARCTests" [
        TestGeneration.Arc.FileSystem.generateArcFileSystemTests arcConfig
        TestGeneration.Arc.ISA.generateISATests arcConfig
    ]
    |> Expecto.performTest
    |> Expecto.writeJUnitSummary (Path.Combine(outPath, "arc-validate-results.xml"))

with
    | :? ArguParseException as ex ->
        match ex.ErrorCode with
        | ErrorCode.HelpText  -> printfn "%s" (CLIArgs.cliArgParser.PrintUsage())
        | _ -> printfn "%s" ex.Message
    | ex ->
        printfn "Internal Error:"
        printfn "%s" ex.Message
        printfn "%A" ex.StackTrace
