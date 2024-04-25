namespace ARCValidate.CLICommands

open ARCValidate.CLIArguments
open Argu

type PackageCommand =
    | [<CliPrefix(CliPrefix.None); AltCommandLine("i")>] Install of ParseResults<PackageInstallArgs>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("u")>] UnInstall of ParseResults<PackageUninstallArgs>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("l")>] List of ParseResults<PackageListArgs>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("c"); SubCommand()>] Update_Index

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Install _     -> "install valiation packages"
            | UnInstall _   -> "uninstall valiation packages"
            | List _        -> "list packages from available soures"
            | Update_Index  -> "update the locally chached package index"