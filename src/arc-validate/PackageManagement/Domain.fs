namespace ARCValidate.PackageManagement

open System.IO
open ValidationPackage.Model

/// Represents a locally installed validation package.
type CachedValidationPackage =
    {
        FileName: string
        CacheDate: System.DateTimeOffset
        LocalPath: string
        Metadata: ValidationPackageMetadata
    }
    with
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

        static member ofPackageMetadata (
            packageMetadata: ValidationPackageMetadata,
            ?Date: System.DateTimeOffset,
            ?CacheFolder: string
        ) =
            let path = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER())

            let fileName =
                match packageMetadata.ProgrammingLanguage.ToLowerInvariant() with
                | "fsharp" ->
                    $"{packageMetadata.Name}@{ValidationPackageMetadata.getSemanticVersionString packageMetadata}.fsx"
                | "python" ->
                    $"{packageMetadata.Name}@{ValidationPackageMetadata.getSemanticVersionString packageMetadata}.py"
                | language -> failwithf $"unknown programming language {language}"

            CachedValidationPackage.create(
                fileName = fileName,
                cacheDate = defaultArg Date System.DateTimeOffset.Now,
                localPath = Path.Combine(path, fileName).Replace("\\", "/"),
                metadata = packageMetadata
            )

        static member updateCacheDate (date: System.DateTimeOffset) (package: CachedValidationPackage) =
            { package with CacheDate = date }

        static member getSemanticVersionString(package: CachedValidationPackage) =
            ValidationPackageMetadata.getSemanticVersionString package.Metadata

        member this.PrettyPrint() =
            $" {this.Metadata.Name} @ version {ValidationPackageMetadata.getSemanticVersionString this.Metadata}{System.Environment.NewLine}{this.Metadata.Description}{System.Environment.NewLine}CacheDate: {this.CacheDate}{System.Environment.NewLine}Installed at: {this.LocalPath}{System.Environment.NewLine}"
