namespace ARCValidate.CLIArguments
open Argu

type PackageInstallArgs = 
    | [<ExactlyOnce; MainCommand>] Package of package_name:string
    | [<Unique; AltCommandLine("-pv")>] PackageVersion of package_version:string
    | [<Unique; AltCommandLine("-pr")>] Preview

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Package _        -> "name of the validation package to install"
            | PackageVersion _        -> "version of the validation package to install. If no version is specified, the latest version will be installed."
            | Preview        -> "install the preview version of the package"

type PackageUninstallArgs = 
    | [<ExactlyOnce; MainCommand>] Package of package_name:string
    | [<Unique; AltCommandLine("-pv")>] PackageVersion of package_version:string
    | [<Unique; AltCommandLine("-pr")>] Preview

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Package _        -> "name of the validation package to uninstall"
            | PackageVersion _        -> "version of the validation package to uninstall. If no version is specified, all versions will be uninstalled."
            | Preview       -> "uninstall the preview version of the package"

type PackageListArgs = 
    | [<AltCommandLine("-i"); Unique>] Installed
    | [<AltCommandLine("-c"); Unique>] Indexed

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Installed     -> "list installed packages from the package cache"
            | Indexed       -> "list indexed packages from the cached package index"