namespace ARCValidate.CLICommands

open ARCValidate.CLIArguments
open Argu

[<HelpFlags([|"--help"; "-h"|])>]
type ARCValidateCommand =
    // Parameters
    | [<Unique>] Verbose    
    | [<Unique; AltCommandLine("-t")>] Token of string

    //Commands
    | [<Unique; CliPrefix(CliPrefix.None); AltCommandLine("v")>] Validate of ParseResults<ValidateArgs>

    // SubCommands
    | [<Unique; CliPrefix(CliPrefix.None); AltCommandLine("p")>] Package of ParseResults<PackageCommand>

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Verbose         -> "Use verbose error messages (with full error stack)."
            | Token _         -> "The token to use for authentication with github."
            | Validate _      -> "command for performing arc validation"
            | Package _       -> "subcommands for validation packages"

    static member createParser() =

        let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)

        ArgumentParser.Create<ARCValidateCommand>(programName = "arc-validate", errorHandler = errorHandler)