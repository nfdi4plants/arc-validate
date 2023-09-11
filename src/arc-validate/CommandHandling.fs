namespace ARCValidate

open ARCValidate.API

module CommandHandling =

    open ARCValidate.CLICommands

    let handlePackageSubCommand (verbose: bool) (command: PackageCommand) = 
        match command with
        | Install args      -> 
            if verbose then printfn "Command: install"
            PackageAPI.install verbose args
        | UnInstall args    -> 
            if verbose then printfn "Command: uninstall"
            PackageAPI.uninstall verbose args
        | List args         -> 
            if verbose then printfn "Command: list"
            PackageAPI.list verbose args
        | Update_Index      -> 
            if verbose then printfn "Command: update-index"
            PackageAPI.updateIndex verbose

    let handleARCValidateCommand (verbose:bool) command = 
        match command with
        | ARCValidateCommand.Validate subcommand -> 
            if verbose then printfn "Command: validate"
            ValidateAPI.validate verbose (subcommand)

        | ARCValidateCommand.Package subcommand -> 
            if verbose then printfn "Subcommand: package"
            handlePackageSubCommand verbose (subcommand.GetSubCommand())

        | _ -> ExitCode.ArgParseError