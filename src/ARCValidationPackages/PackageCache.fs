namespace ARCValidationPackages

open System.Collections.Generic
open System.IO
open System.Text.Json

type PackageCache =
    inherit Dictionary<string, ARCValidationPackage>

    new () = { inherit Dictionary<string, ARCValidationPackage>() }

    new (packages: IEnumerable<KeyValuePair<string,ARCValidationPackage>>) = { inherit Dictionary<string, ARCValidationPackage>(packages) }

    new (cache: PackageCache) = { inherit Dictionary<string, ARCValidationPackage>(cache) }

    new(packages: seq<string * ARCValidationPackage>) = 
        let kv = packages |> Seq.map (fun (k,v) -> KeyValuePair.Create(k,v))
        PackageCache(kv)

    static member create (packages: seq<string * ARCValidationPackage>) =
        new PackageCache(packages)

    static member getPackage (name: string) (cache: PackageCache) =
        cache[name]

    static member tryGetPackage (name: string) (cache: PackageCache) =
        if cache.ContainsKey(name) then Some cache[name] else None

    static member addPackage (package: ARCValidationPackage) (cache: PackageCache) =
        cache.Add(package.Name, package)
        cache

    static member cachePackageByName (packageName: string, ?Path: string, ?Date: System.DateTimeOffset) =
        fun (cache: PackageCache) ->
            let package = ARCValidationPackage.ofPackageName(packageName, ?Path = Path)
            cache 
            |> PackageCache.addPackage package

    static member cachePackageOfIndex (packageIndex: ValidationPackageIndex, ?Date: System.DateTimeOffset) =
        fun (cache: PackageCache) ->
            let package = ARCValidationPackage.ofPackageIndex(packageIndex, ?Date = Date)
            cache.Add(package.Name, package)
            cache

    static member updateCacheDate (name: string) (date: System.DateTimeOffset) (cache: PackageCache) =
        let package = cache.[name]
        cache.[name] <- package |> ARCValidationPackage.updateCacheDate date
        cache

    static member tryUpdateCacheDate (name: string) (date: System.DateTimeOffset) (cache: PackageCache) =
        if cache.ContainsKey(name) then
            let package = cache.[name]
            cache.[name] <- package |> ARCValidationPackage.updateCacheDate date
        cache

    static member removePackage (name: string) (cache: PackageCache) =
        cache.Remove(name) |> ignore
        cache

    static member exists (?Path: string) =
        let path = defaultArg Path (Defaults.PACKAGE_CACHE_FILE_PATH())
        File.Exists(path)

    static member read (?Path: string) =
        let path = defaultArg Path (Defaults.PACKAGE_CACHE_FILE_PATH())
        path
        |> File.ReadAllText
        |> fun jsonString -> JsonSerializer.Deserialize<PackageCache>(jsonString, Defaults.SERIALIZATION_OPTIONS)

    static member get (?Path: string) =
        if PackageCache.exists(?Path = Path) then
            PackageCache.read()
        else
            PackageCache.create([])

    static member write (?Path: string) =
        fun (cache: PackageCache) ->
            let path = defaultArg Path (Defaults.PACKAGE_CACHE_FILE_PATH())
            JsonSerializer.Serialize(cache, Defaults.SERIALIZATION_OPTIONS)
            |> fun json -> File.WriteAllText(path, json)
