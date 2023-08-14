namespace ARCValidationPackages

type ARCValidationPackage =
    {
        Name: string
        LastUpdated: System.DateTimeOffset
        LocalPath: string option
        IsCached: bool 
    } with
        static member create (
            name: string, 
            lastUpdated: System.DateTimeOffset, 
            ?LocalPath: string,
            ?IsCached: bool
        ) = 
            { 
                Name = name
                LastUpdated = lastUpdated
                LocalPath = LocalPath
                IsCached = defaultArg IsCached false
            }