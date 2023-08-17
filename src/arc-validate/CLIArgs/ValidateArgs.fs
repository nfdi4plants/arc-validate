namespace ARCValidate.CLIArguments
open Argu

type ValidateArgs = 
    | [<AltCommandLine("-p")>] ARC_Directory of path:string
    | [<AltCommandLine("-o")>] Out_Directory of path:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Out_Directory _ -> "Optional. Specify a output directory for the test results file (arc-validate-results.xml). Default: file gets written to the arc root folder."
            | ARC_Directory _ -> "Optional. Specify a directory that contains the arc to convert. Default: content of the ARC_PATH environment variable. If ARC_PATH is not set: current directory."
