namespace ARCValidate.CLIArguments
open Argu

type ValidateArgs = 
    | [<AltCommandLine("-i")>] ARC_Directory of path:string
    | [<AltCommandLine("-o")>] Out_Directory of path:string
    | [<AltCommandLine("-p")>] Package of package_name:string
    | [<AltCommandLine("-pv")>] PackageVersion of package_version:string

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | Out_Directory _ -> "Optional. Specify a output directory for the test results file (arc-validate-results.xml). Default: file gets written to the arc root folder."
            | ARC_Directory _ -> "Optional. Specify a directory that contains the arc to convert. Default: content of the ARC_PATH environment variable. If ARC_PATH is not set: current directory."
            | Package _       -> "Optional. Specify a validation package to use on top of the default validation for invenio export. Default: no package is used, only structural validation for invenio export."
            | PackageVersion _ -> "Optional. Specify a version of the validation package to use. If no version is specified, the latest version will be used."