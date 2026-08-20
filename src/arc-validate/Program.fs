module Main

open Argu
open System.IO
open ARCExpect

open ARCValidate
open ARCValidate.CLICommands
open ARCValidate.CLIArguments
open ARCValidate.CommandHandling
open Spectre.Console

[<EntryPoint>]
let main argv =

    let parser = ARCValidateCommand.createParser()

    try
        let splitArguments = PackageArgumentBoundary.split argv
        let args = parser.ParseCommandLine(inputs = splitArguments.CLIArguments)

        let verbose = args.TryGetResult(ARCValidateCommand.Verbose) |> Option.isSome

        let command = args.GetSubCommand()

        let packageBoundaryError =
            match command with
            | ARCValidateCommand.Validate validateArguments ->
                match
                    ValidateArgs.validateCombination
                        validateArguments
                        splitArguments.HasBoundary
                with
                | Ok () -> None
                | Error message -> Some message
            | _ when splitArguments.HasBoundary ->
                Some "The '--' package-argument boundary is only valid for the validate command."
            | _ -> None

        match packageBoundaryError with
        | Some message ->
            $"[red]Argument parsing error:[/] {Markup.Escape message}"
            |> AnsiConsole.MarkupLine
            ExitCode.ArgParseError |> int
        | None ->
            handleARCValidateCommand verbose splitArguments.PackageArguments command
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
