namespace ARCValidate.CLIArguments
open Argu

type ValidateArgs = 
    | [<Unique; AltCommandLine("-i")>] ARC_Directory of path:string
    | [<Unique; AltCommandLine("-o")>] Out_Directory of path:string
    | [<Unique; AltCommandLine("-p")>] Package of package_name:string
    | [<Unique; AltCommandLine("-v")>] Package_Version of package_version:string
    | [<Unique>] Specification_Version of specification_version: string
    | [<Unique>] Source_Branch of branch: string
    | [<Unique>] Source_Commit_Hash of commit_hash: string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Out_Directory _ -> "Optional. Specify a output directory for the test results file (arc-validate-results.xml). Default: file gets written to the arc root folder."
            | ARC_Directory _ -> "Optional. Specify a directory that contains the arc to convert. Default: content of the ARC_PATH environment variable. If ARC_PATH is not set: current directory."
            | Package _       -> "Optional. Specify a validation package. Package-defined arguments may follow the command's '--' boundary."
            | Package_Version _ -> "Optional. Specify a version of the validation package to use. If no version is specified, the latest version will be used."
            | Specification_Version _ -> "Optional. Only has an effect if no package is specified via '-p' Specify a version of the ARC specification to validate against. Default: 'latest'."
            | Source_Branch _ -> "Optional. Record the source branch in generated validation outputs."
            | Source_Commit_Hash _ -> "Optional. Record the source commit hash in generated validation outputs."
