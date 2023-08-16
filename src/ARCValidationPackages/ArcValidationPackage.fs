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
        /// Creates a new ARCValidationPackage from a ValidationPackageIndex, with the CacheDate set to the current or optionally a custom date, and the LocalPath set to the default cache folder.
        /// </summary>
        /// <param name="packageIndex">The input package index entry</param>
        /// yparam name="Date">Optional. The date to set the CacheDate to. Defaults to the current date.</param>
        static member ofPackageIndex (packageIndex: ValidationPackageIndex, ?Date: System.DateTimeOffset) =
            ARCValidationPackage.create(
                name = packageIndex.Name,
                cacheDate = (defaultArg Date System.DateTimeOffset.Now),
                localPath = (Path.Combine(Defaults.PACKAGE_CACHE_FOLDER(), $"{packageIndex.Name}.fsx").Replace("\\","/"))
            )


        /// <summary>
        /// returns a copy of the input ARCValidationPackage with the CacheDate set to the given date.
        /// </summary>
        /// <param name="date">The date to set the CacheDate to</param>
        /// <param name="package">The input package</param>
        static member updateCacheDate (date: System.DateTimeOffset) (package: ARCValidationPackage) =
            {package with CacheDate = date}