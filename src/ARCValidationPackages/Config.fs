namespace ARCValidationPackages
open System.IO
open System.Text.Json
open AVPRIndex.Domain

type Config = {
    PackageIndex: ValidationPackageIndex []
    IndexLastUpdated: System.DateTimeOffset
    PackageCacheFolderPreview: string
    PackageCacheFolderRelease: string
    ConfigFilePath: string
} with
    static member create (
        packageIndex: ValidationPackageIndex [],
        indexLastUpdated: System.DateTimeOffset,
        packageCacheFolderPreview: string,
        packageCacheFolderRelease: string,
        configFilePath: string
    ) =
        {
            PackageIndex = packageIndex
            IndexLastUpdated = indexLastUpdated
            PackageCacheFolderPreview = packageCacheFolderPreview
            PackageCacheFolderRelease = packageCacheFolderRelease
            ConfigFilePath = configFilePath
        }

    static member initDefault(?Token, ?ConfigPath, ?CacheFolderPreview, ?CacheFolderRelease) = 
        Config.create(
            packageIndex = GitHubAPI.getPackageIndex(?Token = Token),
            indexLastUpdated = System.DateTimeOffset.Now,
            packageCacheFolderPreview = defaultArg CacheFolderPreview (Defaults.PACKAGE_CACHE_FOLDER_PREVIEW()),
            packageCacheFolderRelease = defaultArg CacheFolderRelease (Defaults.PACKAGE_CACHE_FOLDER_RELEASE()),
            configFilePath = defaultArg ConfigPath (Defaults.CONFIG_FILE_PATH())
        )
    static member indexContainsPackages (packageName: string) (config: Config) =
        config.PackageIndex |> Array.exists (fun package -> package.Metadata.Name = packageName)

    static member indexContainsPackage (packageName: string) (semverString: string) (config: Config) =
        config.PackageIndex 
        |> Array.exists (fun package -> 
            package.Metadata.Name = packageName 
            && (ValidationPackageIndex.getSemanticVersionString package = semverString)

        )

    static member getIndexedPackagesByName (packageName: string) (config: Config) =
        config.PackageIndex |> Array.filter (fun package -> package.Metadata.Name = packageName)

    static member tryGetLatestPackage (packageName: string) (config: Config) =
        if Config.indexContainsPackages packageName config then
            config 
            |> Config.getIndexedPackagesByName packageName
            |> Array.maxBy ValidationPackageIndex.getSemanticVersionString
            |> Some
        else None

    static member tryGetIndexedPackageByNameAndVersion (packageName: string) (semverString: string) (config: Config) =
        if Config.indexContainsPackage packageName semverString config then
            config.PackageIndex 
            |> Array.find (fun package -> 
                package.Metadata.Name = packageName
                && (ValidationPackageIndex.getSemanticVersionString package = semverString)
            )
            |> Some
        else
            None

    static member getLatestPackage (packageName: string) (config: Config) =
        config 
        |> Config.getIndexedPackagesByName packageName
        |> Array.maxBy ValidationPackageIndex.getSemanticVersionString

    static member getIndexedPackageByNameAndVersion (packageName: string) (semverString: string) (config: Config) =
        config.PackageIndex 
        |> Array.find (fun package -> 
            package.Metadata.Name = packageName
            && (ValidationPackageIndex.getSemanticVersionString package = semverString)
        )

    static member withIndex (index: ValidationPackageIndex []) (config: Config) =
        {
            config with
                PackageIndex = index
        }

    static member exists (?Path: string) =
        let path = defaultArg Path (Defaults.CONFIG_FILE_PATH())
        File.Exists(path)

    static member read (?Path: string) =
        let path = defaultArg Path (Defaults.CONFIG_FILE_PATH())
        path
        |> File.ReadAllText
        |> fun jsonString -> JsonSerializer.Deserialize<Config>(jsonString, Defaults.SERIALIZATION_OPTIONS)

    static member get (?Path: string, ?CacheFolderPreview:string, ?CacheFolderRelease:string, ?Token:string) =
        if Config.exists(?Path = Path) then
            Config.read(?Path = Path)
        else
            Config.initDefault(?Token = Token, ?ConfigPath = Path, ?CacheFolderPreview = CacheFolderPreview, ?CacheFolderRelease = CacheFolderRelease)

    static member write (?Path: string) =
        fun (config: Config) ->
            let path = defaultArg Path config.ConfigFilePath
            System.IO.FileInfo(path).Directory.Create(); // ensures directory exists
            JsonSerializer.Serialize(config, Defaults.SERIALIZATION_OPTIONS)
            |> fun json -> File.WriteAllText(path, json)