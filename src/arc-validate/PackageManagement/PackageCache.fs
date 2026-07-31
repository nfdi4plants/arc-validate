namespace ARCValidate.PackageManagement

open System.Collections.Generic
open System.IO
open System.Text.Json
open ValidationPackage.Model

type PackageCache =
    inherit Dictionary<string, Dictionary<string, CachedValidationPackage>>

    new() =
        { inherit Dictionary<string, Dictionary<string, CachedValidationPackage>>() }

    new(packages: IEnumerable<KeyValuePair<string, Dictionary<string, CachedValidationPackage>>>) =
        { inherit Dictionary<string, Dictionary<string, CachedValidationPackage>>(packages) }

    new(cache: PackageCache) =
        let packages =
            cache
            |> Seq.map (fun entry -> KeyValuePair.Create(entry.Key, Dictionary(entry.Value)))

        PackageCache(packages)

    new(packages: seq<CachedValidationPackage>) =
        let entries =
            packages
            |> Seq.groupBy (fun package -> package.Metadata.Name)
            |> Seq.map (fun (name, versions) ->
                let versionEntries =
                    versions
                    |> Seq.map (fun package ->
                        KeyValuePair.Create(CachedValidationPackage.getSemanticVersionString package, package))
                    |> Dictionary

                KeyValuePair.Create(name, versionEntries))

        PackageCache(entries)

    static member create(packages: seq<CachedValidationPackage>) = PackageCache(packages)

    static member getPackage (name: string) (version: string) (cache: PackageCache) =
        cache[name][version]

    static member getLatestPackage (name: string) (cache: PackageCache) =
        cache[name]
        |> Seq.choose (fun entry ->
            match SemVer.tryParse entry.Key with
            | Some semver when semver.PreRelease = "" && semver.BuildMetadata = "" -> Some (semver, entry.Value)
            | _ -> None)
        |> Seq.maxBy (fun (semver, _) -> semver.Major, semver.Minor, semver.Patch)
        |> snd

    static member getPackages (name: string) (cache: PackageCache) = cache[name]

    static member getAllPackages(cache: PackageCache) =
        cache.Values |> Seq.collect _.Values

    static member tryGetPackage (name: string) (version: string) (cache: PackageCache) =
        match cache.TryGetValue(name) with
        | true, versions ->
            match versions.TryGetValue(version) with
            | true, package -> Some package
            | false, _ -> None
        | false, _ -> None

    static member tryGetLatestPackage (name: string) (cache: PackageCache) =
        match cache.TryGetValue(name) with
        | false, _ -> None
        | true, versions ->
            versions
            |> Seq.choose (fun entry ->
                match SemVer.tryParse entry.Key with
                | Some semver when semver.PreRelease = "" && semver.BuildMetadata = "" -> Some (semver, entry.Value)
                | _ -> None)
            |> Seq.sortByDescending (fun (semver, _) -> semver.Major, semver.Minor, semver.Patch)
            |> Seq.tryHead
            |> Option.map snd

    static member tryGetPackages (name: string) (cache: PackageCache) =
        match cache.TryGetValue(name) with
        | true, packages -> Some packages
        | false, _ -> None

    static member addPackage (package: CachedValidationPackage) (cache: PackageCache) =
        let version = CachedValidationPackage.getSemanticVersionString package

        match cache.TryGetValue(package.Metadata.Name) with
        | true, versions -> versions[version] <- package
        | false, _ ->
            cache[package.Metadata.Name] <- Dictionary([ KeyValuePair.Create(version, package) ])

        cache

    static member updateCacheDate name version date (cache: PackageCache) =
        cache[name][version] <- cache[name][version] |> CachedValidationPackage.updateCacheDate date
        cache

    static member tryUpdateCacheDate name version date (cache: PackageCache) =
        match PackageCache.tryGetPackage name version cache with
        | Some _ -> PackageCache.updateCacheDate name version date cache
        | None -> cache

    static member removePackage name version (cache: PackageCache) =
        match cache.TryGetValue(name) with
        | true, versions ->
            versions.Remove(version) |> ignore
            if versions.Count = 0 then cache.Remove(name) |> ignore
        | false, _ -> ()

        cache

    static member removePackages name (cache: PackageCache) =
        cache.Remove(name) |> ignore
        cache

    static member exists path = File.Exists(path)

    static member read path =
        path
        |> File.ReadAllText
        |> fun json -> JsonSerializer.Deserialize<PackageCache>(json, Defaults.SERIALIZATION_OPTIONS)
        |> Option.ofObj
        |> Option.defaultValue (PackageCache())

    static member get(folder: string, ?FileName: string) =
        let fileName = defaultArg FileName Defaults.PACKAGE_CACHE_FILE_NAME
        let path = Path.Combine(folder, fileName)

        if PackageCache.exists(path) then PackageCache.read(path)
        else PackageCache.create([])

    static member write(folder: string, ?FileName: string) =
        fun (cache: PackageCache) ->
            let fileName = defaultArg FileName Defaults.PACKAGE_CACHE_FILE_NAME
            let path = Path.Combine(folder, fileName)

            JsonSerializer.Serialize(cache, Defaults.SERIALIZATION_OPTIONS)
            |> AtomicFile.writeAllText path
