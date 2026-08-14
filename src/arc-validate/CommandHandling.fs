namespace ARCValidate

open ARCValidate.API

module CommandHandling =

    open ARCValidate.CLICommands

    let handlePackageSubCommand (verbose: bool) (command: PackageCommand) = 
        match command with
        | Install args      -> 
            if verbose then printfn "Command: install"
            PackageAPI.Install(args, verbose)
        | UnInstall args    -> 
            if verbose then printfn "Command: uninstall"
            PackageAPI.Uninstall(args, verbose)
        | List -> 
            if verbose then printfn "Command: list"
            PackageAPI.List(verbose)

    let handleConfigSubCommand (verbose: bool) (command: ConfigCommand) =
        match command with
        | Resolve args -> ConfigAPI.resolve args verbose

    let handleARCValidateCommand
        (verbose: bool)
        (packageArguments: string array)
        command
        =
        match command with
        | ARCValidateCommand.Validate subcommand -> 
            if verbose then printfn "Command: validate"
            ValidateAPI.validate verbose subcommand packageArguments

        | ARCValidateCommand.Package subcommand -> 
            if verbose then printfn "Subcommand: package"
            handlePackageSubCommand verbose (subcommand.GetSubCommand())

        | ARCValidateCommand.Config subcommand ->
            handleConfigSubCommand verbose (subcommand.GetSubCommand())

        | _ -> failwith $"unrecognized command '{command}"
