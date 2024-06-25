namespace ARCValidate.API

open ARCValidate
open ARCValidate.CLIArguments
open ARCValidate.CLICommands
open ARCExpect
open ARCValidationPackages
open ARCValidationPackages.API
open AVPRIndex.Domain

open Argu

type PackageAPI = 

    static member printPackageInstallError (e: PackageInstallError) =
        match e with
        | PackageNotFound p -> printfn $"Package {p} not found. Your package index might be out of date. Consider updating the index via arc-validate package update-index."
        | DownloadError (p, msg) -> printfn $"Error downloading package {p}: {msg}."
        | PackageVersionNotFound (p, v) -> printfn $"Package {p} version {v} not found."
        | PackageInstallError.APIError e -> 
            match e with
            | RateLimitExceeded msg -> printfn $"Rate limit exceeded: {msg}"
            | SerializationError msg -> printfn $"Serialization error: {msg}"
            | NotFoundError msg -> printfn $"Not found: {msg}"

    static member printGetSyncedConfigAndCacheError (e: GetSyncedConfigAndCacheError) =
         match e with
         | SyncError msg -> printfn $"Error syncing config and cache: {msg}"
         | GetSyncedConfigAndCacheError.APIError e -> 
            match e with
            | RateLimitExceeded msg -> printfn $"Rate limit exceeded: {msg}"
            | SerializationError msg -> printfn $"Serialization error: {msg}"
            | NotFoundError msg -> printfn $"Not found: {msg}"

    static member Install(
        args: ParseResults<PackageInstallArgs>,
        ?Verbose: bool,
        ?Token: string
    ) = 
        let isRelease = args.TryGetResult(PackageInstallArgs.Preview).IsSome |> not
        match Common.GetSyncedConfigAndCache(?Token = Token) with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, avprCache, previewCache) -> 
        
            let packageName = args.TryGetResult(PackageInstallArgs.Package).Value
            let version = args.TryGetResult(PackageInstallArgs.Version)

            if Verbose.IsSome then 
                if Verbose.Value then
                    if version.IsSome then
                        printfn $"""Installing package {packageName} {version.Value} from source {if isRelease then "'avpr.nfdi4plants.org'" else "'preview-index on GitHub'"} """
                    else 
                        printfn $"""Installing package latest version of {packageName} from source {if isRelease then "'avpr.nfdi4plants.org'" else "'preview-index on GitHub'"} """
            if isRelease then
                match (AVPR.InstallPackage(avprCache, packageName, ?SemVer = version, ?Verbose = Verbose)) with
                | Ok msg ->
                    printfn $"{msg}"
                    ExitCode.Success

                | Error e ->
                    PackageAPI.printPackageInstallError e
                    ExitCode.InternalError
            else
                match (Preview.InstallPackage(config, previewCache, packageName, ?SemVer = version, ?Verbose = Verbose, ?Token = Token)) with
                | Ok msg ->
                    printfn $"{msg}"
                    ExitCode.Success

                | Error e ->
                    PackageAPI.printPackageInstallError e
                    ExitCode.InternalError

    static member Uninstall(
        args: ParseResults<PackageUninstallArgs>,
        ?Verbose: bool,
        ?Token: string
    ) = 
    
        let isRelease = args.TryGetResult(PackageUninstallArgs.Preview).IsSome |> not
        match Common.GetSyncedConfigAndCache(?Token = Token) with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, avprCache, previewCache) -> 
            let verbose = defaultArg Verbose false
            let packageName = args.TryGetResult(PackageUninstallArgs.Package).Value
            let version = args.TryGetResult(PackageUninstallArgs.PackageVersion)

            if isRelease then
                match (AVPR.UninstallPackage(avprCache, packageName, ?SemVer = version, Verbose = verbose)) with
                | Ok msg ->
                    printfn $"{msg}"
                    ExitCode.Success
                | Error e ->
                    match e with
                    | PackageNotInstalled msg ->
                        printfn $"{msg}"
                        ExitCode.Success
                    | IOError m ->
                        printfn $"Error uninstalling package {packageName}."
                        if verbose then printfn $"{m}"
                        ExitCode.InternalError
            else
                match (Preview.UninstallPackage(previewCache, packageName, ?SemVer = version, Verbose = verbose)) with
                | Ok msg ->
                    printfn $"{msg}"
                    ExitCode.Success
                | Error e ->
                    match e with
                    | PackageNotInstalled msg ->
                        printfn $"{msg}"
                        ExitCode.Success
                    | IOError m ->
                        printfn $"Error uninstalling package {packageName}."
                        if verbose then printfn $"{m}"
                        ExitCode.InternalError

    static member List(
        args: ParseResults<PackageListArgs>,
        ?Verbose: bool, 
        ?Token: string
    ) = 

        match Common.GetSyncedConfigAndCache(?Token = Token) with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, avprCache, previewCache) -> 
            let verbose = defaultArg Verbose false
            let printIndexed=  args.TryGetResult(PackageListArgs.Include_Indexed).IsSome

            let printCachedPackageList (verbose: bool) (packages: seq<CachedValidationPackage>) =
                packages
                |> fun p -> 
                    if Seq.length p = 0 then 
                        printfn $"No validation packages installed."
                    else 
                        if verbose then
                            p |> Seq.iteri (fun i p -> printfn $"{System.Environment.NewLine}[{i}]: {p.PrettyPrint()}")
                        else
                            p 
                            |> Seq.groupBy (fun p -> p.Metadata.Name)
                            |> Seq.iter (fun (name,packages) ->
                                printfn $"- {name}:"
                                packages 
                                |> Seq.sortByDescending (fun p -> ValidationPackageMetadata.getSemanticVersionString p.Metadata)
                                |> Seq.iter (fun p -> printfn $"  - {ValidationPackageMetadata.getSemanticVersionString p.Metadata}")
                            )

            let printIndexedPackageList (verbose: bool) (packages: ValidationPackageIndex list) =
                packages
                |> fun p -> 
                    if p.Length = 0 then 
                        printfn $"No validation packages indexed."
                    else 
                        if verbose then
                            p |> List.iteri (fun i p -> printfn $"{System.Environment.NewLine}[{i}]: {p.PrettyPrint()}")
                        else
                            p 
                            |> List.groupBy (fun p -> p.Metadata.Name)
                            |> List.iter (fun (name,packages) ->
                                printfn $"- {name}:"
                                packages 
                                |> List.sortByDescending (fun p -> ValidationPackageMetadata.getSemanticVersionString p.Metadata)
                                |> List.iter (fun p -> printfn $"  - {ValidationPackageMetadata.getSemanticVersionString p.Metadata}")
                            )
                        


            let installedAVPR = Common.ListCachedPackages(avprCache, verbose)
            let installedPreview = Common.ListCachedPackages(previewCache, verbose)
            let indexedPreview = Preview.ListIndexedPackages(config, verbose)

            match (installedAVPR, installedPreview, indexedPreview) with
            | Ok avpr, Ok preview, Ok indexed ->
                printfn ""
                printfn $"Installed from: avpr.nfdi4plants.org" 
                printCachedPackageList verbose avpr
                printfn ""
                printfn $"Installed from: preview index"
                printCachedPackageList verbose preview
                printfn ""

                if printIndexed then
                    printfn $"Locally indexed preview validation packages:"
                    printIndexedPackageList verbose indexed
                    printfn ""

                ExitCode.Success
            | Error e, _, _ ->
                printfn $"Error listing installed avpr packages."
                if verbose then printfn $"{e}"
                ExitCode.InternalError
            | _, Error e, _ ->
                printfn $"Error listing installed preview packages."
                if verbose then printfn $"{e}"
                ExitCode.InternalError
            | _, _, Error e ->
                printfn $"Error listing indexed packages."
                if verbose then printfn $"{e}"
                ExitCode.InternalError

    static member UpdateIndex(
        ?Verbose: bool,
        ?Token: string
    ) = 
        match Common.GetSyncedConfigAndCache(?Token = Token) with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, _, _) -> 
            let verbose = defaultArg Verbose false

            match Preview.UpdateIndex(config, ?Token = Token) with
            | Ok _ ->
                printfn $"Updated package index."
                ExitCode.Success
            | Error e ->
                printfn $"Error updating package index."
                if verbose then printfn $"{e}"
                ExitCode.InternalError