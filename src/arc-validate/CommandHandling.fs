namespace ARCValidate

open ARCValidate.API

module CommandHandling =

    open ARCValidate.CLICommands

    let handlePackageSubCommand (verbose: bool) (token: string option) (command: PackageCommand) = 
        match command with
        | Install args      -> 
            if verbose then printfn "Command: install"
            PackageAPI.Install(args, verbose, ?Token = token)
        | UnInstall args    -> 
            if verbose then printfn "Command: uninstall"
            PackageAPI.Uninstall(args, verbose)
        | List args         -> 
            if verbose then printfn "Command: list"
            PackageAPI.List(args, verbose, ?Token = token)
        | Update_Index      -> 
            if verbose then printfn "Command: update-index"
            PackageAPI.UpdateIndex(verbose, ?Token = token)

    let handleARCValidateCommand (verbose:bool) (token: string option) command = 
        match command with
        | ARCValidateCommand.Validate subcommand -> 
            if verbose then printfn "Command: validate"
            ValidateAPI.validate verbose token (subcommand)

        | ARCValidateCommand.Package subcommand -> 
            if verbose then printfn "Subcommand: package"
            handlePackageSubCommand verbose token (subcommand.GetSubCommand())

        | _ -> ExitCode.ArgParseError