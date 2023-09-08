namespace ARCValidationPackages

open System.IO

type ARCValidationPackage =
    {
        Name: string
        CacheDate: System.DateTimeOffset
        LocalPath: string
    } with
        static member create (
            name: string, 
            cacheDate: System.DateTimeOffset, 
            localPath: string
        ) = 
            { 
                Name = name
                CacheDate = cacheDate
                LocalPath = localPath
            }

        /// <summary>
        /// Creates a new ARCValidationPackage from a ValidationPackageIndex, with the CacheDate set to the current or optionally a custom date, and the LocalPath set to the default cache folder or custom folder.
        /// </summary>
        /// <param name="packageIndex">The input package index entry</param>
        /// yparam name="Date">Optional. The date to set the CacheDate to. Defaults to the current date.</param>
        static member ofPackageIndex (packageIndex: ValidationPackageIndex, ?Date: System.DateTimeOffset, ?Path: string) =
            let path = defaultArg Path (Defaults.PACKAGE_CACHE_FOLDER())
            ARCValidationPackage.create(
                name = packageIndex.Name,
                cacheDate = (defaultArg Date System.DateTimeOffset.Now),
                localPath = (System.IO.Path.Combine(path, $"{packageIndex.Name}.fsx").Replace("\\","/"))
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
                name = packageName,
                cacheDate = (defaultArg Date System.DateTimeOffset.Now),
                localPath = (System.IO.Path.Combine(path, $"{packageName}.fsx").Replace("\\","/"))
            )

        /// <summary>
        /// returns a copy of the input ARCValidationPackage with the CacheDate set to the given date.
        /// </summary>
        /// <param name="date">The date to set the CacheDate to</param>
        /// <param name="package">The input package</param>
        static member updateCacheDate (date: System.DateTimeOffset) (package: ARCValidationPackage) =
            {package with CacheDate = date}