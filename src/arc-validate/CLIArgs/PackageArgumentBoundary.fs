namespace ARCValidate.CLIArguments

type PackageArgumentSplit =
    {
        CLIArguments: string array
        PackageArguments: string array
        HasBoundary: bool
    }

[<RequireQualifiedAccess>]
module PackageArgumentBoundary =

    let split (arguments: string array) =
        match arguments |> Array.tryFindIndex ((=) "--") with
        | Some index ->
            {
                CLIArguments = arguments[.. index - 1]
                PackageArguments = arguments[index + 1 ..]
                HasBoundary = true
            }
        | None ->
            {
                CLIArguments = arguments
                PackageArguments = Array.empty
                HasBoundary = false
            }

[<RequireQualifiedAccess>]
module PackageProcessArguments =

    let create
        (arcDirectory: string)
        (outputDirectory: string)
        (sourceBranch: string option)
        (sourceCommitHash: string option)
        (packageArguments: string array)
        =
        [|
            "-i"
            arcDirectory
            "-o"
            outputDirectory

            match sourceBranch with
            | Some branch ->
                "--source-branch"
                branch
            | None -> ()

            match sourceCommitHash with
            | Some commitHash ->
                "--source-commit-hash"
                commitHash
            | None -> ()

            yield! packageArguments
        |]
