namespace ARCValidationPackages

open System.Collections.Generic
open System.IO
open System.Text.Json

type PackageCache =
    inherit Dictionary<string, Dictionary<string,ARCValidationPackage>>

    new () = { inherit Dictionary<string, Dictionary<string,ARCValidationPackage>>() }

    new (packages: IEnumerable<KeyValuePair<string, Dictionary<string,ARCValidationPackage>>>) = { inherit Dictionary<string, Dictionary<string,ARCValidationPackage>>(packages) }

    new (cache: PackageCache) = { inherit Dictionary<string, Dictionary<string,ARCValidationPackage>>(cache) }

    new(packages: seq<ARCValidationPackage>) = 
        let kv = 
            packages
            |> Seq.groupBy (fun p -> p.Metadata.Name)
            |> Seq.map (fun (name,packages) -> 
                name, 
                packages
                |> Seq.map (fun p -> KeyValuePair.Create(ARCValidationPackage.getSemanticVersionString p, p) )
                |> Dictionary
            )
            |> Seq.map (fun (name,versions) -> KeyValuePair.Create(name,versions))
        PackageCache(kv)

    static member create (packages: seq<ARCValidationPackage>) =
        new PackageCache(packages)

    static member getPackage (name: string) (semVerString: string) (cache: PackageCache) =
        cache[name][semVerString]

    static member getPackages (name: string) (cache: PackageCache) =
        cache[name]

    static member getAllPackages (cache: PackageCache) =
        cache.Values |> Seq.toList

    static member tryGetPackage (name: string) (semVerString: string) (cache: PackageCache) =
        if cache.ContainsKey(name) then
            if cache[name].ContainsKey(semVerString) then
                cache
                |> PackageCache.getPackage name semVerString
                |> Some
            else 
                None
        else
            None

    static member tryGetPackages (name: string) (cache: PackageCache) =
        if cache.ContainsKey(name) then
            cache
            |> PackageCache.getPackages name
            |> Some
        else 
            None

    static member addPackage (package: ARCValidationPackage) (cache: PackageCache) =

        let semver = ARCValidationPackage.getSemanticVersionString package

        if cache.ContainsKey(package.Metadata.Name) then
            cache[package.Metadata.Name].Add(semver, package)
        else
            cache.Add(
                package.Metadata.Name, 
                new Dictionary<string, ARCValidationPackage>([KeyValuePair.Create(semver, package)])
            )

        cache

    static member cachePackageOfIndex (packageIndex: ValidationPackageIndex, ?Date: System.DateTimeOffset) =
        fun (cache: PackageCache) ->
            cache
            |> PackageCache.addPackage (ARCValidationPackage.ofPackageIndex(packageIndex, ?Date = Date))

    static member updateCacheDate (name: string) (semVerString: string) (date: System.DateTimeOffset) (cache: PackageCache) =
        let package = cache.[name][semVerString]
        cache.[name][semVerString] <- package |> ARCValidationPackage.updateCacheDate date
        cache

    static member tryUpdateCacheDate (name: string) (semVerString: string) (date: System.DateTimeOffset) (cache: PackageCache) =
        if cache.ContainsKey(name) then
            if cache[name].ContainsKey(semVerString) then
                cache
                |> PackageCache.updateCacheDate name semVerString date
            else cache
        else cache

    static member removePackage (name: string) (semVerString: string) (cache: PackageCache) =
        cache[name].Remove(semVerString) |> ignore
        cache

    static member removePackages (name: string) (cache: PackageCache) =
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

    static member get (?Folder: string, ?FileName: string) =
        let fileName = defaultArg FileName (Defaults.PACKAGE_CACHE_FILE_NAME)
        let folder = defaultArg Folder (Defaults.PACKAGE_CACHE_FOLDER())
        let path = Path.Combine(folder, fileName)
        if PackageCache.exists(path) then
            PackageCache.read(path)
        else
            PackageCache.create([])

    static member write (?Folder: string, ?FileName: string) =
        fun (cache: PackageCache) ->
            let fileName = defaultArg FileName (Defaults.PACKAGE_CACHE_FILE_NAME)
            let folder = defaultArg Folder (Defaults.PACKAGE_CACHE_FOLDER())
            let path = Path.Combine(folder, fileName)
            System.IO.FileInfo(path).Directory.Create(); // ensures directory exists
            JsonSerializer.Serialize(cache, Defaults.SERIALIZATION_OPTIONS)
            |> fun json -> File.WriteAllText(path, json)
