namespace ARCValidate.CLIArguments
open Argu

type PackageInstallArgs = 
    | [<Last; ExactlyOnce; MainCommand>] Pakage of package_name:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Pakage _        -> "name of the validation package to install"

type PackageUninstallArgs = 
    | [<Last; ExactlyOnce; MainCommand>] Pakage of package_name:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Pakage _        -> "name of the validation package to uninstall"

type PackageListArgs = 
    | [<AltCommandLine("-i"); Unique>] Installed
    | [<AltCommandLine("-c"); Unique>] Indexed

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Installed     -> "list installed packages from the package cache"
            | Indexed       -> "list indexed packages from the cached package index"

type IndexArgs = 
    | [<Last; AltCommandLine("-u"); ExactlyOnce;>] Update

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Update     -> "update the package index"