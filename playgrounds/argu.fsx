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
    | [<AltCommandLine("-i")>] Installed
    | [<AltCommandLine("-c")>] Indexed

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Installed     -> "list installed packages from the package cache"
            | Indexed       -> "list indexed packages from the cached package index"

type IndexArguments = 
    | [<Last; AltCommandLine("-u"); ExactlyOnce;>] Update

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Update     -> "update the package index"

type PackageArguments =
    | [<CliPrefix(CliPrefix.None); AltCommandLine("i")>] Install of ParseResults<PackageInstallArguments>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("u")>] UnInstall of ParseResults<PackageUninstallArguments>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("l")>] List of ParseResults<PackageListArguments>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("c")>] Index of ParseResults<IndexArguments>

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Install _     -> "install valiation packages"
            | UnInstall _   -> "uninstall valiation packages"
            | List _        -> "list packages from available soures"
            | Index _       -> "update the locally chached package index"

type ValidateArguments = 
    | [<AltCommandLine("-p")>] ARC_Directory of path:string
    | [<AltCommandLine("-o")>] Out_Directory of path:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Out_Directory _ -> "Optional. Specify a output directory for the test results file (arc-validate-results.xml). Default: file gets written to the arc root folder."
            | ARC_Directory _ -> "Optional. Specify a directory that contains the arc to convert. Default: content of the ARC_PATH environment variable. If ARC_PATH is not set: current directory."

[<HelpFlags([|"--help"; "-h"|])>]
type ArcValidateArguments =
    | [<AltCommandLine("-v")>] Verbose
    | [<CliPrefix(CliPrefix.None); AltCommandLine("v")>] Validate of ParseResults<ValidateArguments>
    | [<CliPrefix(CliPrefix.None); AltCommandLine("p")>] Package of ParseResults<PackageArguments>

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Package _       -> "subcommand for validation packages"
            | Validate _      -> "subcommand for performing arc validation"
            | Verbose         -> "Use verbose error messages (with full error stack)."

let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)

let cliArgParser = ArgumentParser.Create<ArcValidateArguments>(programName = "arc-validate", errorHandler = errorHandler)

let handlePackageCommand command = 
    match command with
    | Install args      -> printfn $"  Subcommand install{System.Environment.NewLine}    Arguments: {(args.GetAllResults())}"
    | UnInstall args    -> printfn $"  Subcommand uninstall{System.Environment.NewLine}    Arguments: {(args.GetAllResults())}"
    | List args         -> printfn $"  Subcommand list{System.Environment.NewLine}    Arguments: {(args.GetAllResults())}"
    | Index args        -> printfn $"  Subcommand index{System.Environment.NewLine}    Arguments: {(args.GetAllResults())}"


let handleValidateCommand (args: ParseResults<ValidateArguments>) = 
    printfn "  Arguments: %A" (args.GetAllResults())

let handleArcValidateCommand command = 
    match command with
    | Package subcommand -> 
        printfn "Subcommand package"
        handlePackageCommand (subcommand.GetSubCommand())
    | Validate subcommand -> 
        printfn "Subcommand validate"
        handleValidateCommand (subcommand)
    | Verbose -> printfn "Verbose"

let testCommandLine (args:string[]) =
    try 
        cliArgParser.Parse(args)
            .GetSubCommand()
            |> handleArcValidateCommand

    with e -> printfn "%s" e.Message

testCommandLine [|"validate"|]
