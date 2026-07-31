namespace ARCValidationPackages
open System.IO
open System.Text.Json

type Config = {
    PackageCacheFolder: string
    ConfigFilePath: string
} with
    static member create (
        packageCacheFolder: string,
        configFilePath: string
    ) =
        {
            PackageCacheFolder = packageCacheFolder
            ConfigFilePath = configFilePath
        }

    static member initDefault(?ConfigPath, ?CacheFolder) = 
        Config.create(
            packageCacheFolder = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER()),
            configFilePath = defaultArg ConfigPath (Defaults.CONFIG_FILE_PATH())
        )

    static member exists (?Path: string) =
        let path = defaultArg Path (Defaults.CONFIG_FILE_PATH())
        File.Exists(path)

    static member read (?Path: string) =
        let path = defaultArg Path (Defaults.CONFIG_FILE_PATH())
        path
        |> File.ReadAllText
        |> fun jsonString -> JsonSerializer.Deserialize<Config>(jsonString, Defaults.SERIALIZATION_OPTIONS)

    static member get (?Path: string, ?CacheFolder:string) =
        if Config.exists(?Path = Path) then
            Config.read(?Path = Path)
        else
            Config.initDefault(?ConfigPath = Path, ?CacheFolder = CacheFolder)

    static member write (?Path: string) =
        fun (config: Config) ->
            let path = defaultArg Path config.ConfigFilePath
            System.IO.FileInfo(path).Directory.Create(); // ensures directory exists
            JsonSerializer.Serialize(config, Defaults.SERIALIZATION_OPTIONS)
            |> fun json -> File.WriteAllText(path, json)
