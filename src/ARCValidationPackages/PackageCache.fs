namespace ARCValidationPackages

open System.Collections.Generic
open System.IO
open System.Text.Json
open AVPRIndex.Domain

type PackageCache =
    inherit Dictionary<string, Dictionary<string,CachedValidationPackage>>

    new () = { inherit Dictionary<string, Dictionary<string,CachedValidationPackage>>() }

    new (packages: IEnumerable<KeyValuePair<string, Dictionary<string,CachedValidationPackage>>>) = { inherit Dictionary<string, Dictionary<string,CachedValidationPackage>>(packages) }

    new (cache: PackageCache) = 
        let kv = 
            cache
            |> Seq.map (fun innerKV -> 
                innerKV.Key, Dictionary(innerKV.Value)
            )
            |> Seq.map (fun (name,versions) -> KeyValuePair.Create(name,versions))
        PackageCache(kv)
        

    new(packages: seq<CachedValidationPackage>) = 
        let kv = 
            packages
            |> Seq.groupBy (fun p -> p.Metadata.Name)
            |> Seq.map (fun (name,packages) -> 
                name, 
                packages
                |> Seq.map (fun p -> KeyValuePair.Create(CachedValidationPackage.getSemanticVersionString p, p) )
                |> Dictionary
            )
            |> Seq.map (fun (name,versions) -> KeyValuePair.Create(name,versions))
        PackageCache(kv)

    //override this.Equals(obj: obj) =
    //    match obj with
    //    | :? PackageCache as other -> 
    //        let countIsEqual = this.Count = other.Count
    //        let KeysAreEqual = 
    //            Seq.zip (this.Keys |> Seq.sort) (other.Keys |> Seq.sort)
    //            |> Seq.forall (fun (a,b) -> a = b)
    //        if KeysAreEqual && countIsEqual then
    //            [for key in this.Keys -> this[key], other[key]]
    //            |> Seq.forall(fun (v1, v2) ->
    //                let countIsEqual = v1.Count = v2.Count
    //                let KeysAreEqual = 
    //                    Seq.zip (v1.Keys |> Seq.sort) (v2.Keys |> Seq.sort)
    //                    |> Seq.forall (fun (a,b) -> a = b)
    //                let valuesAreEqual = 
    //                    [for key in v1.Keys -> v1[key], v2[key]]
    //                    |> Seq.forall(fun (v1, v2) -> v1 = v2)
    //                countIsEqual && KeysAreEqual && valuesAreEqual
    //            )
    //        else
    //            false
    //    | _ -> false

    static member create (packages: seq<CachedValidationPackage>) =
        new PackageCache(packages)

    static member getPackage (name: string) (semVerString: string) (cache: PackageCache) =
        cache[name][semVerString]

    static member getLatestPackage (name: string) (cache: PackageCache) =
        cache[name]
        // only get stable versions when suffixes are not supplied explicitly
        |> Seq.filter (fun (kv:KeyValuePair<string,CachedValidationPackage>) ->
            let semver = SemVer.tryParse kv.Key 
            if semver.IsSome then 
                semver.Value.PreRelease = "" 
                && semver.Value.BuildMetadata = ""
            else false
        )
        |> Seq.maxBy (fun (kv:KeyValuePair<string,CachedValidationPackage>) -> 
            let semver = SemVer.tryParse kv.Key |> Option.get
            semver.Major, semver.Minor, semver.Patch
        )
        |> fun x -> x.Value

    static member getPackages (name: string) (cache: PackageCache) =
        cache[name]

    static member getAllPackages (cache: PackageCache) =
        cache.Values 
        |> Seq.toList
        |> Seq.map (fun x -> x.Values)
        |> Seq.concat

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

    static member tryGetLatestPackage (name: string) (cache: PackageCache) =
        if cache.ContainsKey(name) then
            cache
            |> PackageCache.getLatestPackage name
            |> Some
        else 
            None

    static member tryGetPackages (name: string) (cache: PackageCache) =
        if cache.ContainsKey(name) then
            cache
            |> PackageCache.getPackages name
            |> Some
        else 
            None

    static member addPackage (package: CachedValidationPackage) (cache: PackageCache) =

        let semver = CachedValidationPackage.getSemanticVersionString package

        if cache.ContainsKey(package.Metadata.Name) then
            cache[package.Metadata.Name].Add(semver, package)
        else
            cache.Add(
                package.Metadata.Name, 
                new Dictionary<string, CachedValidationPackage>([KeyValuePair.Create(semver, package)])
            )

        cache

    static member cachePackageOfIndex (packageIndex: ValidationPackageIndex, ?Date: System.DateTimeOffset) =
        fun (cache: PackageCache) ->
            cache
            |> PackageCache.addPackage (CachedValidationPackage.ofPackageIndex(packageIndex, ?Date = Date))

    static member updateCacheDate (name: string) (semVerString: string) (date: System.DateTimeOffset) (cache: PackageCache) =
        let package = cache.[name][semVerString]
        cache.[name][semVerString] <- package |> CachedValidationPackage.updateCacheDate date
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

    static member exists (path: string) =
        File.Exists(path)

    static member read (path: string) =
        path
        |> File.ReadAllText
        |> fun jsonString -> JsonSerializer.Deserialize<PackageCache>(jsonString, Defaults.SERIALIZATION_OPTIONS)

    static member get (folder: string, ?FileName: string) =
        let fileName = defaultArg FileName (Defaults.PACKAGE_CACHE_FILE_NAME)
        let path = Path.Combine(folder, fileName)
        if PackageCache.exists(path) then
            PackageCache.read(path)
        else
            PackageCache.create([])

    static member write (folder: string, ?FileName: string) =
        fun (cache: PackageCache) ->
            let fileName = defaultArg FileName (Defaults.PACKAGE_CACHE_FILE_NAME)
            let path = Path.Combine(folder, fileName)
            System.IO.FileInfo(path).Directory.Create(); // ensures directory exists
            JsonSerializer.Serialize(cache, Defaults.SERIALIZATION_OPTIONS)
            |> fun json -> File.WriteAllText(path, json)
