namespace ARCValidationPackages

open System.IO
open System.Text.Json
open System.Text.Json.Serialization
open AVPRIndex.Domain
open System.Runtime.CompilerServices


module ValidationPackageMetadata =
    let getSemanticVersionString(m: ValidationPackageMetadata) = $"{m.MajorVersion}.{m.MinorVersion}.{m.PatchVersion}"

module ValidationPackageIndex =
    let getSemanticVersionString(i: ValidationPackageIndex) = $"{i.Metadata.MajorVersion}.{i.Metadata.MinorVersion}.{i.Metadata.PatchVersion}"
/// <summary>
/// represents the locally installed version of a validation package, e.g. the path to the local file and the date it was cached.
/// </summary>
type CachedValidationPackage =
    {
        FileName: string
        CacheDate: System.DateTimeOffset
        LocalPath: string
        Metadata: ValidationPackageMetadata
    } with
        static member create (
            fileName: string, 
            cacheDate: System.DateTimeOffset, 
            localPath: string,
            metadata: ValidationPackageMetadata
        ) = 
            { 
                FileName = fileName
                CacheDate = cacheDate
                LocalPath = localPath
                Metadata = metadata
            }

        /// <summary>
        /// Creates a new ARCValidationPackage from a ValidationPackageIndex, with the CacheDate set to the current or optionally a custom date, and the LocalPath set to the default cache folder or custom folder.
        /// </summary>
        /// <param name="packageIndex">The input package index entry</param>
        /// <param name="Date">Optional. The date to set the CacheDate to. Defaults to the current date.</param>
        static member ofPackageIndex (packageIndex: ValidationPackageIndex, ?Date: System.DateTimeOffset, ?CacheFolder: string) =
            let path = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER_PREVIEW())
            CachedValidationPackage.create(
                fileName = packageIndex.FileName,
                cacheDate = (defaultArg Date System.DateTimeOffset.Now),
                localPath = (System.IO.Path.Combine(path, packageIndex.FileName).Replace("\\","/")),
                metadata = packageIndex.Metadata
            )

        /// <summary>
        /// Creates a new ARCValidationPackage from a package name only, with the CacheDate set to the current or optionally a custom date, and the LocalPath set to the default cache folder or custom folder.
        /// </summary>
        /// <param name="packageName"></param>
        /// <param name="Date"></param>
        /// <param name="Path"></param>
        static member ofPackageName (packageName: string, ?Date: System.DateTimeOffset, ?Path: string) =
            let path = defaultArg Path (Defaults.PACKAGE_CACHE_FOLDER_PREVIEW())
            CachedValidationPackage.create(
                fileName = packageName,
                cacheDate = (defaultArg Date System.DateTimeOffset.Now),
                localPath = (System.IO.Path.Combine(path, $"{packageName}.fsx").Replace("\\","/")),
                metadata = ValidationPackageMetadata()
            )

        /// <summary>
        /// returns a copy of the input ARCValidationPackage with the CacheDate set to the given date.
        /// </summary>
        /// <param name="date">The date to set the CacheDate to</param>
        /// <param name="package">The input package</param>
        static member updateCacheDate (date: System.DateTimeOffset) (package: CachedValidationPackage) =
            {package with CacheDate = date}

        static member getSemanticVersionString(vp: CachedValidationPackage) = $"{vp.Metadata.MajorVersion}.{vp.Metadata.MinorVersion}.{vp.Metadata.PatchVersion}";

        member this.PrettyPrint() =
            $" {this.Metadata.Name} @ version {this.Metadata.MajorVersion}.{this.Metadata.MinorVersion}.{this.Metadata.PatchVersion}{System.Environment.NewLine}{this.Metadata.Description}{System.Environment.NewLine}CacheDate: {this.CacheDate}{System.Environment.NewLine}Installed at: {this.LocalPath}{System.Environment.NewLine}"