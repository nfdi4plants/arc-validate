namespace ARCValidationPackages
open System.IO

type GetSyncedConfigAndCacheError =
| SyncError of msg: string

type UpdateIndexError =
| DownloadError of msg: string

type PackageInstallError = 
| PackageNotFound of package: string
| DownloadError of package: string * msg: string

type PackageUninstallError =
| PackageNotInstalled of msg: string
| IOError of msg: string

type API =

    static member GetSyncedConfigAndCache(
        ?ConfigPath: string,
        ?CacheFolder: string,
        ?CacheFileName: string,
        ?Token: string
    ) =
        try
            let config = Config.get(?Path = ConfigPath, ?CacheFolder= CacheFolder, ?Token = Token)
            config |> Config.write(?Path = ConfigPath)
            let cache = PackageCache.get(?Folder = CacheFolder, ?FileName = CacheFileName)
            cache |> PackageCache.write(?Folder = CacheFolder, ?FileName = CacheFileName)

            Ok (config, cache)
        with e ->
            Error (GetSyncedConfigAndCacheError.SyncError(e.Message))

    static member SaveAndCachePackage (
        cache: PackageCache,
        indexedPackage: ValidationPackageIndex,
        ?CacheFolder: string,
        ?Token: string
    ) =

        let package = ARCValidationPackage.ofPackageIndex(indexedPackage, ?CacheFolder = CacheFolder)
        try 
            GitHubAPI.downloadPackageScript(indexedPackage, ?Token = Token)
            |> fun script -> 
                File.WriteAllText(
                    package.LocalPath,
                    script
                )

            cache
            |> PackageCache.addPackage package
            |> PackageCache.write()

            Ok ($"installed package {package.Name} at {package.LocalPath}")
                
        with e ->
            Error (PackageInstallError.DownloadError(package.Name, e.Message))

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
            Error (UpdateIndexError.DownloadError(e.Message))

    static member InstallPackage(
        config: Config,
        cache: PackageCache,
        packageName: string,
        ?Verbose: bool,
        ?Token: string
    ) =
        let verbose = defaultArg Verbose false

        if (Config.indexContainsPackage packageName config) then
            // package exists on the local index
            let indexedPackage = Config.getIndexedPackageByName packageName config

            if cache.ContainsKey(packageName) then
                //already cached -> update index and check if newer package is available
                let cachedPackage = cache.[packageName]

                if verbose then printfn $"package {packageName} is already cached locally from {cachedPackage.CacheDate}"
                if verbose then printfn $"updating package index and looking for a newer version..."

                match API.UpdateIndex(config, ?Token = Token) with
                | Ok updatedConfig -> 
                    let updatedIndexPackage = Config.getIndexedPackageByName packageName updatedConfig
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

            else 
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
        ?Verbose: bool
    ) =
        let verbose = defaultArg Verbose false

        if verbose then printfn $"uninstalling package {packageName}..."
        if cache.ContainsKey(packageName) then
            if verbose then printfn $"package {packageName} is installed. removing..."
            let package = cache.[packageName]
            try
                if verbose then printfn $"removing {package.LocalPath}..."
                File.Delete(package.LocalPath)
                cache
                |> PackageCache.removePackage packageName
                |> PackageCache.write()
                Ok ($"uninstalled package {packageName} from {package.LocalPath}")
            with e ->
                if verbose then printfn $"failed to remove {package.LocalPath}: {e.Message}"
                Error (IOError e.Message)
        else
            if verbose then printfn $"package {packageName} is not installed."
            Error (PackageNotInstalled packageName)

    static member ListCachedPackages(
        cache: PackageCache,
        ?Verbose: bool
    ) =
        try
            cache 
            |> PackageCache.getPackages
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

