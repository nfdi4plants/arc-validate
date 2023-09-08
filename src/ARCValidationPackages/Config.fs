namespace ARCValidationPackages
open System.IO
open System.Text.Json

type Config = {
    PackageIndex: ValidationPackageIndex []
    IndexLastUpdated: System.DateTimeOffset
    PackageCacheFolder: string
    ConfigFilePath: string
} with
    static member create (
        packageIndex: ValidationPackageIndex [],
        indexLastUpdated: System.DateTimeOffset,
        packageCacheFolder: string,
        configFilePath: string
    ) =
        {
            PackageIndex = packageIndex
            IndexLastUpdated = indexLastUpdated
            PackageCacheFolder = packageCacheFolder
            ConfigFilePath = configFilePath
        }

    static member initDefault() = 
        Config.create(
            packageIndex = GitHubAPI.getPackageIndex(),
            indexLastUpdated = System.DateTimeOffset.Now,
            packageCacheFolder = Defaults.PACKAGE_CACHE_FOLDER(),
            configFilePath = Defaults.CONFIG_FILE_PATH()
        )

    static member read (?Path: string) =
        let path = defaultArg Path (Defaults.CONFIG_FILE_PATH())
        path
        |> File.ReadAllText
        |> fun jsonString -> JsonSerializer.Deserialize<Config>(jsonString, Defaults.SERIALIZATION_OPTIONS)

    static member write (?Path: string) =
        fun (config: Config) ->
            let path = defaultArg Path config.ConfigFilePath
            JsonSerializer.Serialize(config, Defaults.SERIALIZATION_OPTIONS)
            |> fun json -> File.WriteAllText(config.ConfigFilePath, json)