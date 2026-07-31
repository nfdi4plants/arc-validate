namespace ARCValidate.PackageManagement

open System.IO
open System.Text.Json

type Config =
    {
        PackageCacheFolder: string
        ConfigFilePath: string
        RegistryApiBaseUrl: string
    }
    with
        static member create (
            packageCacheFolder: string,
            configFilePath: string,
            ?registryApiBaseUrl: string
        ) =
            {
                PackageCacheFolder = packageCacheFolder
                ConfigFilePath = configFilePath
                RegistryApiBaseUrl = defaultArg registryApiBaseUrl (Defaults.REGISTRY_API_URL())
            }

        static member initDefault(?ConfigPath, ?CacheFolder, ?RegistryApiBaseUrl) =
            Config.create(
                packageCacheFolder = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER()),
                configFilePath = defaultArg ConfigPath (Defaults.CONFIG_FILE_PATH()),
                registryApiBaseUrl = defaultArg RegistryApiBaseUrl (Defaults.REGISTRY_API_URL())
            )

        static member exists(?Path: string) =
            File.Exists(defaultArg Path (Defaults.CONFIG_FILE_PATH()))

        static member read(?Path: string) =
            let path = defaultArg Path (Defaults.CONFIG_FILE_PATH())

            let config =
                path
                |> File.ReadAllText
                |> fun json -> JsonSerializer.Deserialize<Config>(json, Defaults.SERIALIZATION_OPTIONS)

            {
                config with
                    PackageCacheFolder =
                        if System.String.IsNullOrWhiteSpace(config.PackageCacheFolder) then Defaults.PACKAGE_CACHE_FOLDER()
                        else config.PackageCacheFolder
                    ConfigFilePath =
                        if System.String.IsNullOrWhiteSpace(config.ConfigFilePath) then path
                        else config.ConfigFilePath
                    RegistryApiBaseUrl =
                        if System.String.IsNullOrWhiteSpace(config.RegistryApiBaseUrl) then Defaults.REGISTRY_API_URL()
                        else config.RegistryApiBaseUrl.TrimEnd('/')
            }

        static member get(?Path: string, ?CacheFolder: string, ?RegistryApiBaseUrl: string) =
            let config =
                if Config.exists(?Path = Path) then
                    Config.read(?Path = Path)
                else
                    Config.initDefault(
                        ?ConfigPath = Path,
                        ?CacheFolder = CacheFolder,
                        ?RegistryApiBaseUrl = RegistryApiBaseUrl
                    )

            {
                config with
                    PackageCacheFolder = defaultArg CacheFolder config.PackageCacheFolder
                    RegistryApiBaseUrl = defaultArg RegistryApiBaseUrl config.RegistryApiBaseUrl
            }

        static member write(?Path: string) =
            fun (config: Config) ->
                let path = defaultArg Path config.ConfigFilePath
                JsonSerializer.Serialize(config, Defaults.SERIALIZATION_OPTIONS)
                |> AtomicFile.writeAllText path
