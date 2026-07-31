namespace ARCValidate.API

open System
open Argu
open ARCValidate
open ARCValidate.CLIArguments
open ARCValidate.PackageManagement
open ValidationPackage.Model

type PackageAPI =

    static member printRegistryError(error: RegistryError) =
        match error with
        | NotFound message -> printfn $"Package was not found: {message}"
        | RateLimitExceeded message -> printfn $"Registry rate limit exceeded: {message}"
        | ServerError(statusCode, message) -> printfn $"Registry server error ({statusCode}): {message}"
        | UnexpectedStatus(statusCode, message) -> printfn $"Registry request failed ({statusCode}): {message}"
        | InvalidResponse message -> printfn $"Registry returned an invalid response: {message}"
        | TransportError message -> printfn $"Registry could not be reached: {message}"

    static member printPackageInstallError(error: PackageInstallError) =
        match error with
        | DownloadError(package, message) -> printfn $"Error downloading package {package}: {message}."
        | RegistryRequestError registryError -> PackageAPI.printRegistryError registryError

    static member printGetSyncedConfigAndCacheError(error: GetSyncedConfigAndCacheError) =
        match error with
        | SyncError message -> printfn $"Error syncing config and cache: {message}"

    static member Install(args: ParseResults<PackageInstallArgs>, ?Verbose: bool) =
        match Common.GetSyncedConfigAndCache() with
        | Error error ->
            PackageAPI.printGetSyncedConfigAndCacheError error
            ExitCode.InternalError
        | Ok(config, cache) ->
            let packageName = args.GetResult(PackageInstallArgs.Package)
            let version = args.TryGetResult(PackageInstallArgs.Version)
            let verbose = defaultArg Verbose false

            if verbose then
                match version with
                | Some version -> printfn $"Installing package {packageName} {version}"
                | None -> printfn $"Installing latest version of package {packageName}"

            use registryApi = new RegistryClient(BaseUri = Uri(config.RegistryApiBaseUrl))

            let result =
                AVPR.InstallPackageAsync(
                    cache,
                    packageName,
                    ?SemVer = version,
                    Verbose = verbose,
                    CacheFolder = config.PackageCacheFolder,
                    RegistryApi = registryApi
                )
                |> _.GetAwaiter().GetResult()

            match result with
            | Ok message ->
                printfn $"{message}"
                ExitCode.Success
            | Error error ->
                PackageAPI.printPackageInstallError error
                ExitCode.InternalError

    static member Uninstall(args: ParseResults<PackageUninstallArgs>, ?Verbose: bool) =
        match Common.GetSyncedConfigAndCache() with
        | Error error ->
            PackageAPI.printGetSyncedConfigAndCacheError error
            ExitCode.InternalError
        | Ok(config, cache) ->
            let verbose = defaultArg Verbose false
            let packageName = args.GetResult(PackageUninstallArgs.Package)
            let version = args.TryGetResult(PackageUninstallArgs.PackageVersion)

            match
                AVPR.UninstallPackage(
                    cache,
                    packageName,
                    ?SemVer = version,
                    Verbose = verbose,
                    CacheFolder = config.PackageCacheFolder
                )
            with
            | Ok message ->
                printfn $"{message}"
                ExitCode.Success
            | Error(PackageNotInstalled _) ->
                printfn $"Package {packageName} is not installed."
                ExitCode.Success
            | Error(IOError message) ->
                printfn $"Error uninstalling package {packageName}."
                if verbose then printfn $"{message}"
                ExitCode.InternalError

    static member List(?Verbose: bool) =
        match Common.GetSyncedConfigAndCache() with
        | Error error ->
            PackageAPI.printGetSyncedConfigAndCacheError error
            ExitCode.InternalError
        | Ok(config, cache) ->
            let verbose = defaultArg Verbose false

            let printCachedPackageList (packages: seq<CachedValidationPackage>) =
                if Seq.isEmpty packages then
                    printfn "No validation packages installed."
                elif verbose then
                    packages
                    |> Seq.iteri (fun index package ->
                        printfn $"{Environment.NewLine}[{index}]: {package.PrettyPrint()}")
                else
                    packages
                    |> Seq.groupBy _.Metadata.Name
                    |> Seq.iter (fun (name, versions) ->
                        printfn $"- {name}:"
                        versions
                        |> Seq.sortByDescending (fun package ->
                            ValidationPackageMetadata.getSemanticVersionString package.Metadata)
                        |> Seq.iter (fun package ->
                            printfn $"  - {ValidationPackageMetadata.getSemanticVersionString package.Metadata}"))

            match Common.ListCachedPackages(cache, verbose) with
            | Ok packages ->
                printfn ""
                printfn $"Installed from: {config.RegistryApiBaseUrl}"
                printCachedPackageList packages
                ExitCode.Success
            | Error message ->
                printfn "Error listing installed packages."
                if verbose then printfn $"{message}"
                ExitCode.InternalError
