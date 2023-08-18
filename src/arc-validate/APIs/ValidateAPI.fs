namespace ARCValidate.API

open ARCValidate
open ARCValidate.CLIArguments
open ARCValidate.CLICommands
open ArcValidation
open ARCValidationPackages

open Argu

module ValidateAPI = 

    let validate (verbose: bool) (args: ParseResults<ValidateArgs>) : ExitCode = raise (System.NotImplementedException())