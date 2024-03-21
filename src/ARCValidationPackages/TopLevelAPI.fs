namespace ARCValidationPackages
open System.IO
open AVPRClient
open AVPRIndex
open AVPRIndex.Domain

type APIError = 
| RateLimitExceeded of msg: string
| SerializationError of msg: string
| NotFoundError of msg: string

type GetSyncedConfigAndCacheError =
| SyncError of msg: string
| APIError of APIError

type UpdateIndexError =
| DownloadError of msg: string
| APIError of APIError

type PackageInstallError = 
| PackageNotFound of package: string
| PackageVersionNotFound of package: string * version: string
| DownloadError of package: string * msg: string
| APIError of APIError

type PackageUninstallError =
| PackageNotInstalled of msg: string
| IOError of msg: string
| APIError of APIError

type API =

    static member GetSyncedConfigAndCache(
        release: bool,
        ?ConfigPath: string,
        ?CacheFolderPreview: string,
        ?CacheFolderRelease: string,
        ?CacheFileName: string,
        ?Token: string
    ) =
        try
            let cacheFolderPreview = defaultArg CacheFolderPreview (Defaults.PACKAGE_CACHE_FOLDER_PREVIEW())
            let cacheFolderRelease = defaultArg CacheFolderRelease (Defaults.PACKAGE_CACHE_FOLDER_RELEASE())
            let config = Config.get(?Path = ConfigPath, ?CacheFolderPreview= CacheFolderPreview, ?CacheFolderRelease = CacheFolderRelease, ?Token = Token)
            config |> Config.write(?Path = ConfigPath)
            let cache =
                if release then
                    let c = PackageCache.get(folder = cacheFolderRelease, ?FileName = CacheFileName)
                    c |> PackageCache.write(folder = cacheFolderRelease, ?FileName = CacheFileName)
                    c
                else
                    let c = PackageCache.get(folder = cacheFolderPreview, ?FileName = CacheFileName)
                    c |> PackageCache.write(folder = cacheFolderPreview, ?FileName = CacheFileName)
                    c

            Ok (config, cache)
        with e ->
            Error (GetSyncedConfigAndCacheError.SyncError(e.Message))

    static member SaveAndCachePackage (
        cache: PackageCache,
        indexedPackage: ValidationPackageIndex,
        ?CacheFolder: string,
        ?Token: string
    ) =
        let cacheFolder = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER_PREVIEW())
        let package = CachedValidationPackage.ofPackageIndex(indexedPackage, ?CacheFolder = CacheFolder)
        try 
            GitHubAPI.downloadPackageScript(indexedPackage, ?Token = Token)
            |> fun script -> 
                File.WriteAllText(
                    package.LocalPath,
                    script
                )

            cache
            |> PackageCache.addPackage package
            |> PackageCache.write(cacheFolder)

            Ok ($"installed package {package.FileName} at {package.LocalPath}")
                
        with e ->
            match e with
            | :? GitHubAPI.Errors.RateLimitError as e -> 
                (RateLimitExceeded e.Message)
                |> PackageInstallError.APIError
                |> Error
            | :? GitHubAPI.Errors.SerializationError as e ->
                (SerializationError e.Message)
                |> PackageInstallError.APIError
                |> Error
            | :? GitHubAPI.Errors.NotFoundError as e ->
                (NotFoundError e.Message)
                |> PackageInstallError.APIError
                |> Error
            | _ -> Error (PackageInstallError.DownloadError(package.FileName, e.Message))

    static member UpdateIndex (
        config: Config,
        ?Token: string
    ) =
        try 
            let updatedIndex = GitHubAPI.getPackageIndex(?Token = Token)

            let updatedConfig = 
                config
                |> Config.withIndex updatedIndex

            updatedConfig |> Config.write()

            Ok updatedConfig

        with e ->
            match e with
            | :? GitHubAPI.Errors.RateLimitError as e -> 
                (RateLimitExceeded e.Message)
                |> UpdateIndexError.APIError
                |> Error

            | :? GitHubAPI.Errors.SerializationError as e ->
                (SerializationError e.Message)
                |> UpdateIndexError.APIError
                |> Error

            | :? GitHubAPI.Errors.NotFoundError as e ->
                (NotFoundError e.Message)
                |> UpdateIndexError.APIError
                |> Error

            | _ -> Error (UpdateIndexError.DownloadError(e.Message))

    static member InstallPackage(
        config: Config,
        cache: PackageCache,
        packageName: string,
        ?SemVer: string,
        ?Verbose: bool,
        ?Token: string
    ) =
        let verbose = defaultArg Verbose false
        if (Config.indexContainsPackages packageName config) then
            // package exists on the local index

            let indexedPackage = 
                match SemVer with
                | None ->
                    Config.tryGetLatestPackage packageName config
                | Some version ->
                    Config.tryGetIndexedPackageByNameAndVersion packageName version config
            
            match indexedPackage with
            | None -> Error (PackageVersionNotFound (packageName,(defaultArg SemVer "latest")))
            | Some indexedPackage ->
                match 
                    cache
                    |> PackageCache.tryGetPackage 
                        indexedPackage.Metadata.Name
                        (ValidationPackageIndex.getSemanticVersionString indexedPackage)
                with
                | Some cachedPackage ->
                    //already cached -> update index and check if newer package is available

                    if verbose then printfn $"package {packageName} is already cached locally from {cachedPackage.CacheDate}"
                    if verbose then printfn $"updating package index and looking for a newer version..."

                    match API.UpdateIndex(config, ?Token = Token) with
                    | Ok updatedConfig -> 
                        let updatedIndexPackage = Config.getIndexedPackageByNameAndVersion packageName (CachedValidationPackage.getSemanticVersionString cachedPackage) updatedConfig
                        if updatedIndexPackage.LastUpdated > indexedPackage.LastUpdated then
                            // package on remote index is newer, download package and cache.
                            if verbose then printfn $"package {packageName} is available in a newer version({updatedIndexPackage.LastUpdated} vs {indexedPackage.LastUpdated}). downloading..."
                            API.SaveAndCachePackage(
                                cache = cache,
                                indexedPackage = updatedIndexPackage,
                                ?Token = Token
                            )

                        else
                            // package is installed with latest version
                            Ok ($"package {indexedPackage.FileName} is already installed with the latest version.")

                    | Error e -> 
                        Error (PackageInstallError.DownloadError(packageName, $"failed to update package index: {e}"))

                |None -> 
                    // not cached -> download and cache
                    API.SaveAndCachePackage(
                        cache = cache,
                        indexedPackage = indexedPackage,
                        ?Token = Token
                    )
        else    
            // package does not exists on the local index
            Error (PackageNotFound packageName)

    static member UninstallPackage(
        cache: PackageCache,
        packageName: string,
        ?SemVer: string,
        ?Verbose: bool
    ) =
        let verbose = defaultArg Verbose false

        match SemVer with
        | None ->

            if verbose then printfn $"uninstalling all package versions of {packageName}..."

            try
                PackageCache.getPackages packageName cache
                |> Seq.iter (fun kv -> 
                    let version, package = kv.Key, kv.Value
                    if verbose then printfn $"package {packageName}@{version} is installed. removing..."
                    if verbose then printfn $"removing {package.LocalPath}..."
                    File.Delete(package.LocalPath)
                    cache
                    |> PackageCache.removePackage packageName version
                    |> PackageCache.write(Defaults.PACKAGE_CACHE_FOLDER_PREVIEW())
                )
                Ok ($"uninstalled all package versions of {packageName}")
            with e ->
                if verbose then printfn $"failed to remove a package: {e.Message}"
                Error (IOError e.Message)

        | Some semver ->
            if verbose then printfn $"uninstalling all package versions of {packageName}..."

            match PackageCache.tryGetPackage packageName semver cache with
            | Some p -> 
                if verbose then printfn $"package {packageName}@{semver} is installed. removing..."
                try
                    if verbose then printfn $"removing {p.LocalPath}..."
                    File.Delete(p.LocalPath)
                    cache
                    |> PackageCache.removePackage packageName semver
                    |> PackageCache.write(Defaults.PACKAGE_CACHE_FOLDER_PREVIEW())
                    Ok ($"uninstalled package {packageName}@{semver} from {p.LocalPath}")
                with e ->
                    if verbose then printfn $"failed to remove {p.LocalPath}: {e.Message}"
                    Error (IOError e.Message)

            | None -> 
                if verbose then printfn $"package {packageName} is not installed."
                Error (PackageNotInstalled packageName)


    static member ListCachedPackages(
        cache: PackageCache,
        ?Verbose: bool
    ) =
        try
            cache 
            |> PackageCache.getAllPackages
            |> Ok
        
        with e ->
            Error e.Message

    static member ListIndexedPackages(
        config: Config,
        ?Verbose: bool
    ) =
        try
            config.PackageIndex
            |> Array.toList
            |> Ok
        
        with e ->
            Error e.Message

type AVPR =
    
    static member SaveAndCachePackage (
        cache: PackageCache,
        packageName: string,
        ?packageVersion: string,
        ?CacheFolder: string
    ) =

        try
            let cacheFolder = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER_RELEASE())
            let avprapi = new AVPRAPI()
            
            let validationPackage =
                match packageVersion with
                | Some v ->
                    avprapi.GetPackageByNameAndVersion packageName v
                | None -> 
                    avprapi.GetPackageByName packageName
                    
            let metadata = validationPackage.toValidationPackageMetadata()
            let package = CachedValidationPackage.ofPackageMetadata(metadata, ?CacheFolder = CacheFolder)
            
            cache
            |> PackageCache.addPackage package
            |> PackageCache.write(cacheFolder)

            Ok ($"installed package {package.FileName} at {package.LocalPath}")
                
        with e ->
            match e with
            | _ -> Error (PackageInstallError.DownloadError(packageName, e.Message))
            
    static member InstallPackage(
        config: Config,
        cache: PackageCache,
        packageName: string,
        ?SemVer: string,
        ?Verbose: bool,
        ?Token: string
    ) =
        let verbose = defaultArg Verbose false
        let cachedPackage =
            match SemVer with
            | Some semver -> 
                cache
                |> PackageCache.tryGetPackage packageName semver
            | None ->
                cache
                |> PackageCache.tryGetLatestPackage packageName
            
        match cachedPackage
        with
        | Some cachedPackage ->
            //already cached -> check if newer package is available
            if verbose then printfn $"package {packageName} is already cached locally from {cachedPackage.CacheDate}"
            if verbose then printfn $"updating package index and looking for a newer version..."

            let avprapi = new AVPRAPI()
            
            let latestPackage =
                avprapi.GetPackageByName packageName
            if ValidationPackageMetadata.getSemanticVersionString cachedPackage.Metadata = ValidationPackageMetadata.getSemanticVersionString (latestPackage.toValidationPackageMetadata()) then
                Ok ($"package {packageName} is already installed with the latest version.")
            else
                if verbose then printfn $"package {packageName} is available in a newer version({ValidationPackageMetadata.getSemanticVersionString (latestPackage.toValidationPackageMetadata())} vs {ValidationPackageMetadata.getSemanticVersionString cachedPackage.Metadata}). downloading..."
                AVPR.SaveAndCachePackage(
                    cache = cache,
                    packageName = packageName
                )

        |None -> 
            // not cached -> download and cache
            AVPR.SaveAndCachePackage(
                cache = cache,
                packageName = packageName
            )

    static member UninstallPackage(
        cache: PackageCache,
        packageName: string,
        ?SemVer: string,
        ?Verbose: bool
    ) =
        let verbose = defaultArg Verbose false

        match SemVer with
        | None ->

            if verbose then printfn $"uninstalling all package versions of {packageName}..."

            try
                PackageCache.getPackages packageName cache
                |> Seq.iter (fun kv -> 
                    let version, package = kv.Key, kv.Value
                    if verbose then printfn $"package {packageName}@{version} is installed. removing..."
                    if verbose then printfn $"removing {package.LocalPath}..."
                    File.Delete(package.LocalPath)
                    cache
                    |> PackageCache.removePackage packageName version
                    |> PackageCache.write(Defaults.PACKAGE_CACHE_FOLDER_RELEASE())
                )
                Ok ($"uninstalled all package versions of {packageName}")
            with e ->
                if verbose then printfn $"failed to remove a package: {e.Message}"
                Error (IOError e.Message)

        | Some semver ->
            if verbose then printfn $"uninstalling all package versions of {packageName}..."

            match PackageCache.tryGetPackage packageName semver cache with
            | Some p -> 
                if verbose then printfn $"package {packageName}@{semver} is installed. removing..."
                try
                    if verbose then printfn $"removing {p.LocalPath}..."
                    File.Delete(p.LocalPath)
                    cache
                    |> PackageCache.removePackage packageName semver
                    |> PackageCache.write(Defaults.PACKAGE_CACHE_FOLDER_RELEASE())
                    Ok ($"uninstalled package {packageName}@{semver} from {p.LocalPath}")
                with e ->
                    if verbose then printfn $"failed to remove {p.LocalPath}: {e.Message}"
                    Error (IOError e.Message)

            | None -> 
                if verbose then printfn $"package {packageName} is not installed."
                Error (PackageNotInstalled packageName)

    static member ListCachedPackages(
        cache: PackageCache,
        ?Verbose: bool
    ) =
        try
            cache 
            |> PackageCache.getAllPackages
            |> Ok
        
        with e ->
            Error e.Message