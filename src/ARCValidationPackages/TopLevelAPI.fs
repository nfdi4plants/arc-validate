namespace ARCValidationPackages.API

open ARCValidationPackages
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

/// Top-level API functions that do not depend on wether the package source is the preview index or the AVPR API.
type Common = 

    /// <summary>
    /// Returns the current config and package caches for both preview and avpr packages.
    ///
    /// The return value is a tuple containing (config, avprCache, previewCache).
    /// 
    /// If no custom pathhs for config and/or cache are provided, the default paths are used.
    /// 
    /// Config and caches are read from the provided paths or defaults, if they exist. If they do not exist, they are created.
    /// </summary>
    /// <param name="ConfigPath"></param>
    /// <param name="CacheFolderPreview"></param>
    /// <param name="CacheFolderRelease"></param>
    /// <param name="CacheFileName"></param>
    /// <param name="Token"></param>
    static member GetSyncedConfigAndCache(
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

            let avprCache =
                let c = PackageCache.get(folder = cacheFolderRelease, ?FileName = CacheFileName)
                c |> PackageCache.write(folder = cacheFolderRelease, ?FileName = CacheFileName)
                c

            let previewCache = 
                let c = PackageCache.get(folder = cacheFolderPreview, ?FileName = CacheFileName)
                c |> PackageCache.write(folder = cacheFolderPreview, ?FileName = CacheFileName)
                c

            Ok (config, avprCache, previewCache)
        with e ->
            Error (GetSyncedConfigAndCacheError.SyncError(e.Message))

    /// <summary>
    /// Lists all cached packages in the given cache.
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="Verbose"></param>
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

/// Top-level API functions using the preview index as package source
type Preview = 

    /// <summary>
    /// Syncs the local copy of the preview index with the latest version on remote index.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="Token"></param>
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

    /// <summary>
    /// Lists all indexed packages in the local preview index.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="Verbose"></param>
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

    /// <summary>
    /// Adds the given indexed preview package to the preview cache and writes the cache and package to disk.
    /// </summary>
    /// <param name="cache"></param>
    /// <param name="indexedPackage"></param>
    /// <param name="CacheFolder"></param>
    /// <param name="Token"></param>
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

            Ok ($"installed preview package {package.FileName}@{ValidationPackageMetadata.getSemanticVersionString package.Metadata} at {package.LocalPath}")
                
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

    /// <summary>
    /// Installs the given preview package from the preview index.
    ///
    /// If the a package with the given name (and optional version) does not exist on the local preview index, an error is returned.
    ///
    /// If the package exists on the local preview index, the package is downloaded and installed.
    ///
    /// If no version is provided, the latest version of the package is installed.
    ///
    /// If a package with the given name (and optional version) is already installed, the preview index is updated and the latest version of the package is installed.
    /// </summary>
    /// <param name="config"></param>
    /// <param name="cache"></param>
    /// <param name="packageName"></param>
    /// <param name="SemVer"></param>
    /// <param name="Verbose"></param>
    /// <param name="Token"></param>
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
            // package exists on the local preview index

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

                    match Preview.UpdateIndex(config, ?Token = Token) with
                    | Ok updatedConfig -> 
                        let updatedIndexPackage = Config.getIndexedPackageByNameAndVersion packageName (CachedValidationPackage.getSemanticVersionString cachedPackage) updatedConfig
                        if updatedIndexPackage.LastUpdated > indexedPackage.LastUpdated then
                            // package on remote index is newer, download package and cache.
                            if verbose then printfn $"package {packageName} is available in a newer version({updatedIndexPackage.LastUpdated} vs {indexedPackage.LastUpdated}). downloading..."
                            Preview.SaveAndCachePackage(
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
                    Preview.SaveAndCachePackage(
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
            if verbose then printfn $"uninstalling {packageName}@{semver}..."

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

/// Top-level API functions using the AVPR API (avpr.nfdi4plants.org) as package source
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

            File.WriteAllBytes(package.LocalPath, validationPackage.PackageContent)

            cache
            |> PackageCache.addPackage package
            |> PackageCache.write(cacheFolder)

            Ok ($"installed package {package.FileName} at {package.LocalPath}")
                
        with e ->
            match e with
            | _ -> Error (PackageInstallError.DownloadError(packageName, e.Message))
            
    static member InstallPackage(
        cache: PackageCache,
        packageName: string,
        ?SemVer: string,
        ?Verbose: bool
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