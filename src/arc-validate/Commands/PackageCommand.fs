namespace ARCValidate.CLICommands

open ARCValidate.CLIArguments
open Argu

type PackageCommand =
    | [<SubCommand; CliPrefix(CliPrefix.None); AltCommandLine("i")>] Install of ParseResults<PackageInstallArgs>
    | [<SubCommand; CliPrefix(CliPrefix.None); AltCommandLine("u")>] UnInstall of ParseResults<PackageUninstallArgs>
    | [<SubCommand; CliPrefix(CliPrefix.None); AltCommandLine("l")>] List

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Install _     -> "install valiation packages"
            | UnInstall _   -> "uninstall valiation packages"
            | List          -> "list packages from available soures"