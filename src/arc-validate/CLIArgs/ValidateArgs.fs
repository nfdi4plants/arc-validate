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
    | [<Unique>] Validation_Config of path: string
    | [<Unique>] Validation_Config_Sha256 of sha256: string

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
            | Validation_Config _ -> "Optional. Read configured package inputs from this explicit validation_packages.yml path. Requires --package and --package-version."
            | Validation_Config_Sha256 _ -> "Optional. Require the validation configuration bytes to match this lowercase SHA-256 digest. Requires --validation-config."

[<RequireQualifiedAccess>]
module ValidateArgs =

    let validateCombination (args: ParseResults<ValidateArgs>) hasPackageBoundary =
        let package = args.TryGetResult(ValidateArgs.Package)
        let version = args.TryGetResult(ValidateArgs.Package_Version)
        let config = args.TryGetResult(ValidateArgs.Validation_Config)
        let digest = args.TryGetResult(ValidateArgs.Validation_Config_Sha256)

        match config, digest, package, version, hasPackageBoundary with
        | None, Some _, _, _, _ ->
            Error "--validation-config-sha256 requires --validation-config."
        | Some _, _, None, _, _ ->
            Error "--validation-config requires --package or -p."
        | Some _, _, _, None, _ ->
            Error "--validation-config requires --package-version or -v."
        | Some _, _, _, _, true ->
            Error "--validation-config is mutually exclusive with package arguments after '--'."
        | None, _, None, _, true ->
            Error "Package arguments after '--' require validation with '--package' or '-p'."
        | _ -> Ok()
