#r "nuget: Argu"
open Argu


// ------------------------ Package ARGS -------------------------------------------
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

// ------------------------ Validate ARGS -------------------------------------------

type ValidateArgs = 
    | [<AltCommandLine("-i")>] ARC_Directory of path:string
    | [<AltCommandLine("-o")>] Out_Directory of path:string
    | [<AltCommandLine("-p")>] Package of package_name:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Out_Directory _ -> "Optional. Specify a output directory for the test results file (arc-validate-results.xml). Default: file gets written to the arc root folder."
            | ARC_Directory _ -> "Optional. Specify a directory that contains the arc to convert. Default: content of the ARC_PATH environment variable. If ARC_PATH is not set: current directory."
            | Package _       -> "Optional. Specify a validation package to use on top of the default validation for invenio export. Default: no package is used, only structural validation for invenio export."

// ------------------------ Package Command -----------------------------------------

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

// ---------------------- ARCValidate Command ---------------------------------------

[<HelpFlags([|"--help"; "-h"|])>]
type ArcValidateCommand =
    // Parameters
    | [<AltCommandLine("-v")>] Verbose

    //Commands
    | [<CliPrefix(CliPrefix.None); AltCommandLine("v")>] Validate of ParseResults<ValidateArgs>

    // SubCommands
    | [<CliPrefix(CliPrefix.None); AltCommandLine("p")>] Package of ParseResults<PackageCommand>

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Verbose         -> "Use verbose error messages (with full error stack) and print diagnostic & debug messages"
            | Validate _      -> "command for performing arc validation"
            | Package _       -> "subcommands for validation packages"

    static member createParser() =

        let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)

        ArgumentParser.Create<ArcValidateCommand>(programName = "arc-validate", errorHandler = errorHandler)

// ------------------------------------------------------------------------------------------------------------------------------

let handlePackageInstallCommand (args: ParseResults<PackageInstallArgs>) = 
    printfn $"  command install{System.Environment.NewLine}    Arguments: {(args.GetAllResults())}"

let handlePacageUninstallCommand (args: ParseResults<PackageUninstallArgs>) = 
    printfn $"  command uninstall{System.Environment.NewLine}    Arguments: {(args.GetAllResults())}"

let handlePackageListCommand (args: ParseResults<PackageListArgs>) = 
    printfn $"  command list{System.Environment.NewLine}    Arguments: {(args.GetAllResults())}"

let handlePackageSubCommand command = 
    match command with
    | Install args      -> handlePackageInstallCommand args
    | UnInstall args    -> handlePacageUninstallCommand args
    | List args         -> handlePackageListCommand args
    | Update_Index      -> printfn $"Argument: update-index"

let handleValidateCommand (args: ParseResults<ValidateArgs>) = 
    printfn "  Arguments: %A" (args.GetAllResults())

let handleArcValidateCommand command = 
    match command with
    | Package subcommand -> 
        printfn "Subcommand package"
        handlePackageSubCommand (subcommand.GetSubCommand())
    | Validate subcommand -> 
        printfn "Subcommand validate"
        handleValidateCommand (subcommand)
    | Verbose -> printfn "Argument verbose"

let testCommandLine (args:string[]) =
    try 
        let args = (ArcValidateCommand.createParser()).ParseCommandLine(args)
        let verbose  = args.TryGetResult(Verbose) |> Option.isSome
        printfn $"verbose: {verbose}"
        args
            .GetSubCommand()
            |> handleArcValidateCommand

    with e -> printfn "%s" e.Message

testCommandLine [|"-v"; "validate"; "-p"; "lol"; "-i"; "arcerino"|]

testCommandLine [|"p"; "i"; "soos"|]

testCommandLine [|"p"; "update-index"|]