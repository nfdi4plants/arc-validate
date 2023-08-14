namespace ARCValidationPackages
open System.IO
open System.Text.Json

type Config = {
    PackageIndex: ValidationPackageIndex []
    IndexLastUpdated: System.DateTimeOffset
    PackageCachePath: string
} with
    static member create (
        packageIndex: ValidationPackageIndex [],
        indexLastUpdated: System.DateTimeOffset,
        packageCachePath: string
    ) =
        {
            PackageIndex = packageIndex
            IndexLastUpdated = indexLastUpdated
            PackageCachePath = packageCachePath
        }

    static member initDefault() = 
        Config.create(
            GitHubAPI.getPackageIndex(),
            System.DateTimeOffset.Now,
            Defaults.PACKAGE_CACHE_LOCATION()
        )

    static member read (path: string) =
        path
        |> File.ReadAllText
        |> fun jsonString -> JsonSerializer.Deserialize<Config>(jsonString, Defaults.SERIALIZATION_OPTIONS)

    static member write (path: string) (config: Config) =
        // to-do:
        // - convert to json and write to file
        raise (System.NotImplementedException())