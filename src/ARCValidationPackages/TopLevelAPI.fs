namespace ARCValidationPackages.API

open ARCValidationPackages
open System.IO
open AVPRClient
open AVPRClient.Interop
open ValidationPackage.Model

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

/// Top-level API functions
type Common = 

    /// <summary>
    /// Returns the current config and package caches for avpr packages.
    ///
    /// The return value is a tuple containing (config, avprCache).
    /// 
    /// If no custom pathhs for config and/or cache are provided, the default paths are used.
    /// 
    /// Config and caches are read from the provided paths or defaults, if they exist. If they do not exist, they are created.
    /// </summary>
    /// <param name="ConfigPath"></param>
    /// <param name="CacheFolder"></param>
    /// <param name="CacheFileName"></param>
    /// <param name="Token"></param>
    static member GetSyncedConfigAndCache(
        ?ConfigPath: string,
        ?CacheFolder: string,
        ?CacheFileName: string
    ) =
        try
            let cacheFolderRelease = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER())
            let config = Config.get(?Path = ConfigPath, ?CacheFolder = CacheFolder)
            config |> Config.write(?Path = ConfigPath)

            let avprCache =
                let c = PackageCache.get(folder = cacheFolderRelease, ?FileName = CacheFileName)
                c |> PackageCache.write(folder = cacheFolderRelease, ?FileName = CacheFileName)
                c

            Ok (config, avprCache)
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

/// Top-level API functions using the AVPR API (avpr.nfdi4plants.org) as package source
type AVPR =
    
    static member SaveAndCachePackage (
        cache: PackageCache,
        packageName: string,
        ?packageVersion: string,
        ?CacheFolder: string,
        ?RegistryApi: AVPRAPI
    ) =

        try
            let cacheFolder = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER())
            let avprapi = defaultArg RegistryApi (new AVPRAPI())
            
            let validationPackage =
                match packageVersion with
                | Some v ->
                    avprapi.GetPackageByNameAndVersion packageName v
                | None -> 
                    avprapi.GetPackageByName packageName
                    
            let metadata = validationPackage.ToModel()
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
        ?Verbose: bool,
        ?RegistryApi: AVPRAPI
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

            let avprapi = defaultArg RegistryApi (new AVPRAPI())
            
            let latestPackage =
                avprapi.GetPackageByName packageName
            if ValidationPackageMetadata.getSemanticVersionString cachedPackage.Metadata = ValidationPackageMetadata.getSemanticVersionString (latestPackage.ToModel()) then
                Ok ($"package {packageName} is already installed with the latest version.")
            else
                if verbose then printfn $"package {packageName} is available in a newer version({ValidationPackageMetadata.getSemanticVersionString (latestPackage.ToModel())} vs {ValidationPackageMetadata.getSemanticVersionString cachedPackage.Metadata}). downloading..."
                AVPR.SaveAndCachePackage(
                    cache = cache,
                    packageName = packageName,
                    RegistryApi = avprapi
                )

        |None -> 
            // not cached -> download and cache
            AVPR.SaveAndCachePackage(
                cache = cache,
                packageName = packageName,
                ?packageVersion = SemVer,
                ?RegistryApi = RegistryApi
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
                    |> PackageCache.write(Defaults.PACKAGE_CACHE_FOLDER())
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
                    |> PackageCache.write(Defaults.PACKAGE_CACHE_FOLDER())
                    Ok ($"uninstalled package {packageName}@{semver} from {p.LocalPath}")
                with e ->
                    if verbose then printfn $"failed to remove {p.LocalPath}: {e.Message}"
                    Error (IOError e.Message)

            | None -> 
                if verbose then printfn $"package {packageName} is not installed."
                Error (PackageNotInstalled packageName)
