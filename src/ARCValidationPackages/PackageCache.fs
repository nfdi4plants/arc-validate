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

    static member addOfPackageIndex (packageIndex: ValidationPackageIndex, ?Date: System.DateTimeOffset) =
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

    static member read (path: string) =
        path
        |> File.ReadAllText
        |> fun jsonString -> JsonSerializer.Deserialize<PackageCache>(jsonString, Defaults.SERIALIZATION_OPTIONS)

    static member write (path: string) (cache: PackageCache) =
        JsonSerializer.Serialize(cache, Defaults.SERIALIZATION_OPTIONS)
        |> fun json -> File.WriteAllText(path, json)
