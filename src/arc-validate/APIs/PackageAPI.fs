namespace ARCValidate.API

open ARCValidate.CLIArguments
open ARCValidate.CLICommands
open ArcValidation
open ARCValidationPackages

open Argu

module PackageAPI = 

    let install (verbose: bool) (args: ParseResults<PackageInstallArgs>) : unit = raise (System.NotImplementedException())

    let uninstall (verbose: bool) (args: ParseResults<PackageUninstallArgs>) : unit = raise (System.NotImplementedException())

    let list (verbose: bool)(args: ParseResults<PackageListArgs>) : unit = raise (System.NotImplementedException())

    let updateIndex (verbose: bool) : unit = raise (System.NotImplementedException())

