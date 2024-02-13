namespace ARCValidationPackages

open System.IO
open System.Text.Json
open System.Text.Json.Serialization

// must be a class to be deserializable with YamlDotNet
/// <summary>
/// Represents the metadata of a validation package, e.g. version, name and description.
/// </summary>
type ValidationPackageMetadata() =
    member val Name = "" with get,set
    member val Description = "" with get,set
    member val MajorVersion = 0 with get,set
    member val MinorVersion = 0 with get,set
    member val PatchVersion = 0 with get,set

    override this.GetHashCode() =
        hash (this.Name, this.Description, this.MajorVersion, this.MinorVersion, this.PatchVersion)

    override this.Equals(other) =
        match other with
        | :? ValidationPackageMetadata as vpm -> 
            (this.Name, this.Description, this.MajorVersion, this.MinorVersion, this.PatchVersion) = (vpm.Name, vpm.Description, vpm.MajorVersion, vpm.MinorVersion, vpm.PatchVersion)
        | _ -> false

    static member create (
        name: string, 
        description: string, 
        majorVersion: int, 
        minorVersion: int, 
        patchVersion: int
    ) = 
        let tmp = ValidationPackageMetadata()
        tmp.Name <- name
        tmp.Description <- description
        tmp.MajorVersion <- majorVersion
        tmp.MinorVersion <- minorVersion
        tmp.PatchVersion <- patchVersion
        tmp

    static member getSemanticVersionString(m: ValidationPackageMetadata) = $"{m.MajorVersion}.{m.MinorVersion}.{m.PatchVersion}";

/// <summary>
/// represents a remotely available version of a validation package, e.g. the path to the file on GitHub and the date it was last updated.
/// </summary>
type ValidationPackageIndex =
    {
        RepoPath: string
        FileName:string
        LastUpdated: System.DateTimeOffset
        Metadata: ValidationPackageMetadata
    } with
        static member create (
            repoPath: string, 
            fileName: string, 
            lastUpdated: System.DateTimeOffset,
            metadata: ValidationPackageMetadata

        ) = 
            { 
                RepoPath = repoPath 
                FileName = fileName
                LastUpdated = lastUpdated 
                Metadata = metadata
            }
        static member create (
            repoPath: string, 
            lastUpdated: System.DateTimeOffset,
            metadata: ValidationPackageMetadata
        ) = 
            ValidationPackageIndex.create(
                repoPath = repoPath,
                fileName = Path.GetFileNameWithoutExtension(repoPath),
                lastUpdated = lastUpdated,
                metadata = metadata
            )

        static member getSemanticVersionString(i: ValidationPackageIndex) = $"{i.Metadata.MajorVersion}.{i.Metadata.MinorVersion}.{i.Metadata.PatchVersion}";

        member this.PrettyPrint() =
            $" {this.Metadata.Name} @ version {this.Metadata.MajorVersion}.{this.Metadata.MinorVersion}.{this.Metadata.PatchVersion}{System.Environment.NewLine}{_.Metadata.Description}{System.Environment.NewLine}Last Updated: {this.LastUpdated}{System.Environment.NewLine}"

/// <summary>
/// represents the locally installed version of a validation package, e.g. the path to the local file and the date it was cached.
/// </summary>
type ARCValidationPackage =
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
            let path = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER())
            ARCValidationPackage.create(
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
            let path = defaultArg Path (Defaults.PACKAGE_CACHE_FOLDER())
            ARCValidationPackage.create(
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
        static member updateCacheDate (date: System.DateTimeOffset) (package: ARCValidationPackage) =
            {package with CacheDate = date}

        static member getSemanticVersionString(vp: ARCValidationPackage) = $"{vp.Metadata.MajorVersion}.{vp.Metadata.MinorVersion}.{vp.Metadata.PatchVersion}";

        member this.PrettyPrint() =
            $" {this.Metadata.Name} @ version {this.Metadata.MajorVersion}.{this.Metadata.MinorVersion}.{this.Metadata.PatchVersion}{System.Environment.NewLine}{this.Metadata.Description}{System.Environment.NewLine}CacheDate: {this.CacheDate}{System.Environment.NewLine}Installed at: {this.LocalPath}{System.Environment.NewLine}"