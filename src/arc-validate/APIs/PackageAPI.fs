namespace ARCValidate.API

open ARCValidate
open ARCValidate.CLIArguments
open ARCValidate.CLICommands
open ARCExpect
open ARCValidationPackages
open ARCValidationPackages.API
open ValidationPackage.Model

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
        ?Verbose: bool
    ) = 
        match Common.GetSyncedConfigAndCache() with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, avprCache) -> 
        
            let packageName = args.TryGetResult(PackageInstallArgs.Package).Value
            let version = args.TryGetResult(PackageInstallArgs.Version)

            if Verbose.IsSome then 
                if Verbose.Value then
                    if version.IsSome then
                        printfn $"""Installing package {packageName} {version.Value}"""
                    else 
                        printfn $"""Installing package latest version of {packageName}"""

            match (AVPR.InstallPackage(avprCache, packageName, ?SemVer = version, ?Verbose = Verbose)) with
            | Ok msg ->
                printfn $"{msg}"
                ExitCode.Success

            | Error e ->
                PackageAPI.printPackageInstallError e
                ExitCode.InternalError

    static member Uninstall(
        args: ParseResults<PackageUninstallArgs>,
        ?Verbose: bool
    ) = 
    
        match Common.GetSyncedConfigAndCache() with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, avprCache) -> 
            let verbose = defaultArg Verbose false
            let packageName = args.TryGetResult(PackageUninstallArgs.Package).Value
            let version = args.TryGetResult(PackageUninstallArgs.PackageVersion)

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

    static member List(
        ?Verbose: bool
    ) = 

        match Common.GetSyncedConfigAndCache() with
        | Error e -> 
            PackageAPI.printGetSyncedConfigAndCacheError e
            ExitCode.InternalError

        | Ok (config, avprCache) -> 
            let verbose = defaultArg Verbose false

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

            let installed = Common.ListCachedPackages(avprCache, verbose)

            match installed with
            | Ok cached_packages ->
                printfn ""
                printfn $"Installed from: avpr.nfdi4plants.org" 
                printCachedPackageList verbose cached_packages

                ExitCode.Success
            | Error e ->
                printfn $"Error listing installed packages."
                if verbose then printfn $"{e}"
                ExitCode.InternalError
