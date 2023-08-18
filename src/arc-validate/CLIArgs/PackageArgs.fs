namespace ARCValidate.CLIArguments
open Argu

type PackageInstallArgs = 
    | [<Last; ExactlyOnce; MainCommand>] Package of package_name:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Package _        -> "name of the validation package to install"

type PackageUninstallArgs = 
    | [<Last; ExactlyOnce; MainCommand>] Package of package_name:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Package _        -> "name of the validation package to uninstall"

type PackageListArgs = 
    | [<AltCommandLine("-i"); Unique>] Installed
    | [<AltCommandLine("-c"); Unique>] Indexed

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Installed     -> "list installed packages from the package cache"
            | Indexed       -> "list indexed packages from the cached package index"