namespace ARCValidationPackages

open System.IO

module Errors =     

    type UpdateIndexError =
    | DownloadError of msg: string

    type PackageInstallError = 
    | PackageNotFound of package: string
    | DownloadError of package: string * msg: string

    type PackageAPIError =
        | UpdateIndexError of UpdateIndexError
        | PackageInstallError of PackageInstallError

module API = 

    open Errors

    let saveAndCachePackage (cache: PackageCache) indexedPackage =
        let package = ARCValidationPackage.ofPackageIndex indexedPackage
        try 
            GitHubAPI.downloadPackageScript(indexedPackage)
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

    let updateIndex (config: Config) : Result<Config, UpdateIndexError> =
        try 
            let updatedIndex = GitHubAPI.getPackageIndex()

            let updatedConfig = 
                config
                |> Config.withIndex updatedIndex

            updatedConfig |> Config.write()

            Ok updatedConfig

        with e ->
            Error (UpdateIndexError.DownloadError(e.Message))

    let installPackage (verbose: bool) (config: Config) (cache: PackageCache) (packageName: string) : Result<string, PackageInstallError> =

        if (Config.indexContainsPackage packageName config) then
            // package exists on the local index
            let indexedPackage = Config.getIndexedPackageByName packageName config

            if cache.ContainsKey(packageName) then
                //already cached -> update index and check if newer package is available
                let cachedPackage = cache.[packageName]

                if verbose then printfn $"package {packageName} is already cached locally from {cachedPackage.CacheDate}"
                if verbose then printfn $"updating package index and looking for a newer version..."

                match updateIndex config with
                | Ok updatedConfig -> 
                    let updatedIndexPackage = Config.getIndexedPackageByName packageName updatedConfig
                    if updatedIndexPackage.LastUpdated > indexedPackage.LastUpdated then
                        // package on remote index is newer, download package and cache.
                        if verbose then printfn $"package {packageName} is available in a newer version({updatedIndexPackage.LastUpdated} vs {indexedPackage.LastUpdated}). downloading..."
                        saveAndCachePackage cache updatedIndexPackage

                    else
                        // package is installed with latest version
                        Ok ($"package {indexedPackage.Name} is already installed with the latest version.")

                | Error e -> 
                    Error (PackageInstallError.DownloadError(packageName, $"failed to update package index: {e}"))

            else 
                // not cached -> download and cache
                saveAndCachePackage cache indexedPackage

        else    
            // package does not exists on the local index
            Error (PackageNotFound packageName)