namespace ARCValidate.CLIArguments

open Argu

/// Defines arguments for resolving one validation-package configuration file.
type ConfigResolveArgs =
    | [<ExactlyOnce>] Validation_Config of path: string

    interface IArgParserTemplate with
        member argument.Usage =
            match argument with
            | Validation_Config _ ->
                "Path to the .arc/validation_packages.yml file to preflight."
