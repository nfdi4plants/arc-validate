#r "nuget: Argu"
open Argu

type PackageInstallArguments = 
    | [<Last; ExactlyOnce; MainCommand>] Pakage of package_name:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Pakage _        -> "name of the validation package to install"

type PackageUninstallArguments = 
    | [<Last; ExactlyOnce; MainCommand>] Pakage of package_name:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Pakage _        -> "name of the validation package to uninstall"

type PackageListArguments = 
    | Installed
    | Indexed

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Installed     -> "list installed packages from the package cache"
            | Indexed       -> "list indexed packages from the cached package index"

type PackageArguments =
    | [<CliPrefix(CliPrefix.None)>] Install of ParseResults<PackageInstallArguments>
    | [<CliPrefix(CliPrefix.None)>] UnInstall of ParseResults<PackageUninstallArguments>
    | [<CliPrefix(CliPrefix.None)>] List of ParseResults<PackageListArguments>

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Install _     -> "install valiation packages"
            | UnInstall _   -> "uninstall valiation packages"
            | List _        -> "list packages from available soures"

type ArcValidateArguments =
    | [<AltCommandLine("-v")>] Verbose
    | [<CliPrefix(CliPrefix.None)>] Package of ParseResults<PackageArguments>

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Package _     -> "subcommands for validation packages"
            | Verbose       -> "Use verbose error messages (with full error stack)."

let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)

let cliArgParser = ArgumentParser.Create<ArcValidateArguments>(programName = "arc-validate", errorHandler = errorHandler)

[
    yield cliArgParser.PrintUsage()
    for p in cliArgParser.GetSubCommandParsers()
        do 
            yield p.PrintUsage()
            for p2 in p.GetSubCommandParsers()
                do 
                    yield p2.PrintUsage()
]