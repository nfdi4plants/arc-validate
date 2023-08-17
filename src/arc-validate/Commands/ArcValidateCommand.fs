namespace ARCValidate.CLICommands

open Argu

[<HelpFlags([|"--help"; "-h"|])>]
type ArcValidateCommand =
    | [<AltCommandLine("-v")>] Verbose
    | [<CliPrefix(CliPrefix.None); AltCommandLine("v")>] Validate of ParseResults<ValidateCommand>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("p")>] Package of ParseResults<PackageCommand>

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Package _       -> "subcommand for validation packages"
            | Validate _      -> "subcommand for performing arc validation"
            | Verbose         -> "Use verbose error messages (with full error stack)."

    static member createParser() =

        let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)

        ArgumentParser.Create<ArcValidateCommand>(programName = "arc-validate", errorHandler = errorHandler)