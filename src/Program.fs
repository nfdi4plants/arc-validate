module ARCValidate

open Expecto
open Argu
open System.IO

try
    let args = CLIArgs.cliArgParser.ParseCommandLine()

    let arcPath = 
        args.TryGetResult(CLIArgs.ARC_Directory)
        |> Option.defaultValue (System.Environment.GetEnvironmentVariable("ARC_PATH")) // default to ARC_PATH if argument is not provided
        |> fun s -> if String.isNullOrWhiteSpace s then System.Environment.CurrentDirectory else s // default to ./ if ARC_PATH is not set

    let outPath = 
        args.TryGetResult(CLIArgs.Out_Directory)
        |> Option.defaultValue arcPath

    testList "ARCTests" [
        ValidateArc.filesystem
        ValidateArc.isaTests
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
