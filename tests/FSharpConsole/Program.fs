open ARCValidate
open ARCValidate.CLICommands

let parser = ARCValidateCommand.createParser()

let args = parser.Parse(inputs = [|"--verbose";"package";"install";"test"|])

printfn "%A" args

ARCValidate.CommandHandling.handleARCValidateCommand true None (args.GetSubCommand())