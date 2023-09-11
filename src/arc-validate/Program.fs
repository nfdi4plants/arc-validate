module Main

open Expecto
open Argu
open System.IO
open ARCExpect
open ARCExpect.Configs

open LegacyArgs
open ARCValidate
open ARCValidate.CLICommands
open ARCValidate.CLIArguments
open ARCValidate.CommandHandling

[<EntryPoint>]
let main argv =

    let parser = ARCValidateCommand.createParser()

    try
        let args = parser.ParseCommandLine()

        let verbose = args.TryGetResult(ARCValidateCommand.Verbose) |> Option.isSome
        
        handleARCValidateCommand verbose (args.GetSubCommand())
        |> int

    with
        | :? ArguParseException as ex ->
            match ex.ErrorCode with
            | ErrorCode.HelpText  -> 
                printfn "%s" (parser.PrintUsage())
                ExitCode.Success |> int // printing usage is not an error

            | ErrorCode.CommandLine ->
                printfn "Argument parsing error:"
                printfn "%s" ex.Message
                printfn "%A" ex.StackTrace // might want to add verbosity level to hide this
                ExitCode.ArgParseError |> int

            | _ -> 
                printfn "Internal Error:"
                printfn "%s" ex.Message
                printfn "%A" ex.StackTrace // might want to add verbosity level to hide this
                ExitCode.InternalError |> int
        | ex ->
            printfn "Internal Error:"
            printfn "%s" ex.Message
            printfn "%A" ex.StackTrace // might want to add verbosity level to hide this

            ExitCode.InternalError |> int
            