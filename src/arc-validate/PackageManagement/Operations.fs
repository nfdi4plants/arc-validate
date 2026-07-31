namespace ARCValidate.PackageManagement

open System.IO
open System.Threading
open System.Threading.Tasks
open AVPRClient.Interop
open ValidationPackage.Model

type GetSyncedConfigAndCacheError =
    | SyncError of message: string

type PackageInstallError =
    | DownloadError of package: string * message: string
    | RegistryRequestError of RegistryError

type PackageUninstallError =
    | PackageNotInstalled of package: string
    | IOError of message: string

type Common =

    static member GetSyncedConfigAndCache(
        ?ConfigPath: string,
        ?CacheFolder: string,
        ?CacheFileName: string,
        ?RegistryApiBaseUrl: string
    ) =
        try
            let config =
                Config.get(
                    ?Path = ConfigPath,
                    ?CacheFolder = CacheFolder,
                    ?RegistryApiBaseUrl = RegistryApiBaseUrl
                )

            config |> Config.write(?Path = ConfigPath)

            let cache = PackageCache.get(config.PackageCacheFolder, ?FileName = CacheFileName)
            cache |> PackageCache.write(config.PackageCacheFolder, ?FileName = CacheFileName)

            Ok(config, cache)
        with error ->
            Error(SyncError error.Message)

    static member ListCachedPackages(cache: PackageCache, ?Verbose: bool) =
        try
            cache |> PackageCache.getAllPackages |> Ok
        with error ->
            Error error.Message

type AVPR =

    static member SaveAndCachePackageAsync(
        cache: PackageCache,
        packageName: string,
        ?PackageVersion: string,
        ?CacheFolder: string,
        ?CacheFileName: string,
        ?RegistryApi: RegistryClient,
        ?CancellationToken: CancellationToken
    ) =
        task {
            let cacheFolder = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER())
            let registryApi = defaultArg RegistryApi (new RegistryClient())
            let disposeRegistryApi = RegistryApi.IsNone

            try
                try
                    let! validationPackage =
                        match PackageVersion with
                        | Some version ->
                            registryApi.GetPackageByNameAndVersionAsync(
                                packageName,
                                version,
                                ?cancellationToken = CancellationToken
                            )
                        | None ->
                            registryApi.GetPackageByNameAsync(
                                packageName,
                                ?cancellationToken = CancellationToken
                            )

                    let metadata = validationPackage.ToModel()

                    let package =
                        CachedValidationPackage.ofPackageMetadata(
                            metadata,
                            CacheFolder = cacheFolder
                        )

                    AtomicFile.writeAllBytes package.LocalPath validationPackage.PackageContent

                    cache
                    |> PackageCache.addPackage package
                    |> PackageCache.write(cacheFolder, ?FileName = CacheFileName)

                    return Ok $"installed package {package.FileName} at {package.LocalPath}"
                with
                | RegistryRequestException error -> return Error(RegistryRequestError error)
                | error -> return Error(DownloadError(packageName, error.Message))
            finally
                if disposeRegistryApi then
                    (registryApi :> System.IDisposable).Dispose()
        }

    static member InstallPackageAsync(
        cache: PackageCache,
        packageName: string,
        ?SemVer: string,
        ?Verbose: bool,
        ?CacheFolder: string,
        ?CacheFileName: string,
        ?RegistryApi: RegistryClient,
        ?CancellationToken: CancellationToken
    ) =
        task {
            let verbose = defaultArg Verbose false

            match SemVer with
            | Some version when PackageCache.tryGetPackage packageName version cache |> Option.isSome ->
                return Ok $"package {packageName}@{version} is already installed."
            | _ ->
                match SemVer, PackageCache.tryGetLatestPackage packageName cache with
                | None, Some cachedPackage ->
                    if verbose then
                        printfn $"package {packageName} is already cached locally from {cachedPackage.CacheDate}"
                        printfn "updating package information and looking for a newer version..."

                    let registryApi = defaultArg RegistryApi (new RegistryClient())
                    let disposeRegistryApi = RegistryApi.IsNone

                    try
                        try
                            let! latestPackage =
                                registryApi.GetPackageByNameAsync(
                                    packageName,
                                    ?cancellationToken = CancellationToken
                                )

                            let latestMetadata = latestPackage.ToModel()

                            if CachedValidationPackage.getSemanticVersionString cachedPackage
                               = ValidationPackageMetadata.getSemanticVersionString latestMetadata then
                                return Ok $"package {packageName} is already installed with the latest version."
                            else
                                if verbose then
                                    printfn $"package {packageName} is available in a newer version. downloading..."

                                return!
                                    AVPR.SaveAndCachePackageAsync(
                                        cache,
                                        packageName,
                                        ?CacheFolder = CacheFolder,
                                        ?CacheFileName = CacheFileName,
                                        RegistryApi = registryApi,
                                        ?CancellationToken = CancellationToken
                                    )
                        with
                        | RegistryRequestException error -> return Error(RegistryRequestError error)
                    finally
                        if disposeRegistryApi then
                            (registryApi :> System.IDisposable).Dispose()
                | _ ->
                    return!
                        AVPR.SaveAndCachePackageAsync(
                            cache,
                            packageName,
                            ?PackageVersion = SemVer,
                            ?CacheFolder = CacheFolder,
                            ?CacheFileName = CacheFileName,
                            ?RegistryApi = RegistryApi,
                            ?CancellationToken = CancellationToken
                        )
        }

    static member UninstallPackage(
        cache: PackageCache,
        packageName: string,
        ?SemVer: string,
        ?Verbose: bool,
        ?CacheFolder: string,
        ?CacheFileName: string
    ) =
        let verbose = defaultArg Verbose false
        let cacheFolder = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER())

        let removePackage (package: CachedValidationPackage) =
            if verbose then printfn $"removing {package.LocalPath}..."
            File.Delete(package.LocalPath)

        try
            match SemVer with
            | None ->
                match PackageCache.tryGetPackages packageName cache with
                | None -> Error(PackageNotInstalled packageName)
                | Some packages ->
                    packages.Values |> Seq.toArray |> Array.iter removePackage
                    cache |> PackageCache.removePackages packageName |> PackageCache.write(cacheFolder, ?FileName = CacheFileName)
                    Ok $"uninstalled all package versions of {packageName}"
            | Some version ->
                match PackageCache.tryGetPackage packageName version cache with
                | None -> Error(PackageNotInstalled packageName)
                | Some package ->
                    removePackage package
                    cache |> PackageCache.removePackage packageName version |> PackageCache.write(cacheFolder, ?FileName = CacheFileName)
                    Ok $"uninstalled package {packageName}@{version} from {package.LocalPath}"
        with error ->
            Error(IOError error.Message)
