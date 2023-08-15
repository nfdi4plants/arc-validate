namespace ARCValidationPackages

type ARCValidationPackage =
    {
        Name: string
        LastUpdated: System.DateTimeOffset
        LocalPath: string
    } with
        static member create (
            name: string, 
            lastUpdated: System.DateTimeOffset, 
            localPath: string
        ) = 
            { 
                Name = name
                LastUpdated = lastUpdated
                LocalPath = localPath
            }