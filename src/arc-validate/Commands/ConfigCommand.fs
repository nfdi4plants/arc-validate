namespace ARCValidate.CLICommands

open ARCValidate.CLIArguments
open Argu

/// Defines configuration-oriented CLI subcommands.
type ConfigCommand =
    | [<SubCommand; CliPrefix(CliPrefix.None)>] Resolve of ParseResults<ConfigResolveArgs>

    interface IArgParserTemplate with
        member command.Usage =
            match command with
            | Resolve _ -> "Resolve and preflight a validation configuration as validation_plan.json."
