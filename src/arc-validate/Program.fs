module Main

open Expecto
open Argu
open System.IO
open ARCExpect
open ARCExpect.Configs

open ARCValidate
open ARCValidate.CLICommands
open ARCValidate.CLIArguments
open ARCValidate.CommandHandling
open Spectre.Console

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
                (parser.PrintUsage()) |> AnsiConsole.MarkupLine
                ExitCode.Success |> int // printing usage is not an error

            | ErrorCode.CommandLine ->
                "[red]Argument parsing error:[/]" |> AnsiConsole.MarkupLine
                AnsiConsole.WriteException(ex) // might want to add verbosity level to hide this
                ExitCode.ArgParseError |> int

            | _ -> 
                "[red]Internal Error:[/]" |> AnsiConsole.MarkupLine
                AnsiConsole.WriteException(ex) // might want to add verbosity level to hide this
                ExitCode.InternalError |> int
        | ex ->
            "[red]Internal Error:[/]" |> AnsiConsole.MarkupLine
            AnsiConsole.WriteException(ex) // might want to add verbosity level to hide this

            ExitCode.InternalError |> int
            