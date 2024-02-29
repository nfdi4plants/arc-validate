#r "nuget: Fake.DotNet.Cli, 6.0.0"
#r "nuget: FsHttp, 11.0.0"
#r "nuget: AVPRClient, 0.0.1"

#load "Results.fs"
#load "Defaults.fs"
#load "Domain.fs"
#load "GitHubAPI.fs"
#load "PackageCache.fs"
#load "Config.fs"
#load "ScriptExecution.fs"
#load "TopLevelAPI.fs"

open AVPRClient
open ARCValidationPackages
open System
open System.Net.Http

//let token = 
//    let t = System.Environment.GetEnvironmentVariable("ARC_VALIDATE_GITHUB_API_TEST_TOKEN")
//    if isNull(t) then None else Some t

//let configPath = $"C:/Users/schne/Desktop/lol/{Defaults.CONFIG_FILE_NAME}"
//let cacheFolder = $@"C:/Users/schne/Desktop/lol/cache"

//let syncResult = API.GetSyncedConfigAndCache(ConfigPath = configPath, CacheFolder = cacheFolder, ?Token = token)

//let freshConfigAPI, freshCacheAPI =
//    match syncResult with
//    | Result.Ok (config, cache) -> config, cache
//    | Result.Error e -> failwithf "sync error %A" e

//let anotherIndex = GitHubAPI.getPackageIndex(?Token = token)

//let updatedConfig = 
//    API.UpdateIndex(freshConfigAPI, ?Token = token)

//let saveResults = 
//    API.SaveAndCachePackage(
//        cache = freshCacheAPI,
//        CacheFolder= freshConfigAPI.PackageCacheFolder,
//        indexedPackage = (Array.find (fun p -> p.FileName = "test") freshConfigAPI.PackageIndex),
//        ?Token = token
//    )

let handler = new HttpClientHandler (UseCookies = false)
let baseUri = Uri("https://avpr.nfdi4plants.org")
let httpClient = new HttpClient(handler, true, BaseAddress=baseUri)

let client = AVPRClient.Client httpClient

let allPackages = 
    client.GetAllPackagesAsync(Threading.CancellationToken.None)
    |> Async.AwaitTask
    |> Async.RunSynchronously

let packageCache =
    new PackageCache()


type ValidationPackageMetadataNew() =
    member val Name = "" with get,set
    member val Description = "" with get,set
    member val MajorVersion = 0 with get,set
    member val MinorVersion = 0 with get,set
    member val PatchVersion = 0 with get,set

    override this.GetHashCode() =
        hash (this.Name, this.Description, this.MajorVersion, this.MinorVersion, this.PatchVersion)

    override this.Equals(other) =
        match other with
        | :? ValidationPackageMetadataNew as vpm -> 
            (this.Name, this.Description, this.MajorVersion, this.MinorVersion, this.PatchVersion) = (vpm.Name, vpm.Description, vpm.MajorVersion, vpm.MinorVersion, vpm.PatchVersion)
        | _ -> false

    static member create (
        name: string, 
        description: string, 
        ?majorVersion: int, 
        ?minorVersion: int, 
        ?patchVersion: int
    ) = 
        let tmp = ValidationPackageMetadataNew()
        tmp.Name <- name
        tmp.Description <- description
        match majorVersion,minorVersion,patchVersion with
        | Some major, Some minor, Some patch -> 
            tmp.MajorVersion <- major
            tmp.MinorVersion <- minor
            tmp.PatchVersion <- patch
        | None, Some minor, Some patch -> 
            tmp.MinorVersion <- minor
            tmp.PatchVersion <- patch
        | None, None, Some patch -> 
            tmp.PatchVersion <- patch
        | None, None, None -> ()
        | _ -> failwith "invalid version number"
        tmp

    static member getSemanticVersionString(m: ValidationPackageMetadataNew) = $"{m.MajorVersion}.{m.MinorVersion}.{m.PatchVersion}";

/// <summary>
/// represents a remotely available version of a validation package, e.g. the path to the file on GitHub and the date it was last updated.
/// </summary>
type ValidationPackageIndexNew =
    {
        RepoPath: string option
        FileName:string
        LastUpdated: System.DateTimeOffset
        Metadata: ValidationPackageMetadataNew
    } with
        static member create (
            repoPath: string option, 
            fileName: string, 
            lastUpdated: System.DateTimeOffset,
            metadata: ValidationPackageMetadataNew

        ) = 
            { 
                RepoPath = repoPath 
                FileName = fileName
                LastUpdated = lastUpdated 
                Metadata = metadata
            }
        static member create (
            repoPath: string option, 
            lastUpdated: System.DateTimeOffset,
            metadata: ValidationPackageMetadataNew
        ) = 
            ValidationPackageIndexNew.create(
                repoPath = repoPath,
                fileName = IO.Path.GetFileNameWithoutExtension(repoPath.Value),
                lastUpdated = lastUpdated,
                metadata = metadata
            )

        static member getSemanticVersionString(i: ValidationPackageIndexNew) = $"{i.Metadata.MajorVersion}.{i.Metadata.MinorVersion}.{i.Metadata.PatchVersion}";

        member this.PrettyPrint() =
            $" {this.Metadata.Name} @ version {this.Metadata.MajorVersion}.{this.Metadata.MinorVersion}.{this.Metadata.PatchVersion}{System.Environment.NewLine}{_.Metadata.Description}{System.Environment.NewLine}Last Updated: {this.LastUpdated}{System.Environment.NewLine}"


type ARCValidationPackageNew =
    {
        FileName: string
        CacheDate: System.DateTimeOffset
        LocalPath: string
        Metadata: ValidationPackageMetadataNew
    } with
        static member create (
            fileName: string, 
            cacheDate: System.DateTimeOffset, 
            localPath: string,
            metadata: ValidationPackageMetadataNew
        ) = 
            { 
                FileName = fileName
                CacheDate = cacheDate
                LocalPath = localPath
                Metadata = metadata
            }

        /// <summary>
        /// Creates a new ARCValidationPackage from a ValidationPackageIndex, with the CacheDate set to the current or optionally a custom date, and the LocalPath set to the default cache folder or custom folder.
        /// </summary>
        /// <param name="packageIndex">The input package index entry</param>
        /// <param name="Date">Optional. The date to set the CacheDate to. Defaults to the current date.</param>
        static member ofPackageIndex (packageIndex: ValidationPackageIndexNew, ?Date: System.DateTimeOffset, ?CacheFolder: string) =
            let path = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER())
            ARCValidationPackageNew.create(
                fileName = packageIndex.FileName,
                cacheDate = (defaultArg Date System.DateTimeOffset.Now),
                localPath = (System.IO.Path.Combine(path, packageIndex.FileName).Replace("\\","/")),
                metadata = packageIndex.Metadata
            )

        static member getSemanticVersionString(vp: ARCValidationPackageNew) = $"{vp.Metadata.MajorVersion}.{vp.Metadata.MinorVersion}.{vp.Metadata.PatchVersion}";

 
open System.Collections.Generic
open System.Text.Json
 
type PackageCacheNew =
    inherit Dictionary<string, Dictionary<string,ARCValidationPackageNew>>

    new () = { inherit Dictionary<string, Dictionary<string,ARCValidationPackageNew>>() }

    new (packages: IEnumerable<KeyValuePair<string, Dictionary<string,ARCValidationPackageNew>>>) = { inherit Dictionary<string, Dictionary<string,ARCValidationPackageNew>>(packages) }

    new (cache: PackageCacheNew) = 
        let kv = 
            cache
            |> Seq.map (fun innerKV -> 
                innerKV.Key, Dictionary(innerKV.Value)
            )
            |> Seq.map (fun (name,versions) -> KeyValuePair.Create(name,versions))
        PackageCacheNew(kv)
        

    new(packages: seq<ARCValidationPackageNew>) = 
        let kv = 
            packages
            |> Seq.groupBy (fun p -> p.Metadata.Name)
            |> Seq.map (fun (name,packages) -> 
                name, 
                packages
                |> Seq.map (fun p -> KeyValuePair.Create(ARCValidationPackageNew.getSemanticVersionString p, p) )
                |> Dictionary
            )
            |> Seq.map (fun (name,versions) -> KeyValuePair.Create(name,versions))
        PackageCacheNew(kv)

    static member addPackage (package: ARCValidationPackageNew) (cache: PackageCacheNew) =

        let semver = ARCValidationPackageNew.getSemanticVersionString package

        if cache.ContainsKey(package.Metadata.Name) then
            cache[package.Metadata.Name].Add(semver, package)
        else
            cache.Add(
                package.Metadata.Name, 
                new Dictionary<string, ARCValidationPackageNew>([KeyValuePair.Create(semver, package)])
            )
            
        cache
        
    static member write (?Folder: string, ?FileName: string) =
        fun (cache: PackageCacheNew) ->
            let fileName = defaultArg FileName (Defaults.PACKAGE_CACHE_FILE_NAME)
            let folder = defaultArg Folder (Defaults.PACKAGE_CACHE_FOLDER())
            let path = IO.Path.Combine(folder, fileName)
            System.IO.FileInfo(path).Directory.Create(); // ensures directory exists
            JsonSerializer.Serialize(cache, Defaults.SERIALIZATION_OPTIONS)
            |> fun json -> IO.File.WriteAllText(path, json)

type APINew =
    //static member SaveAndCachePackage (
    //    cache: PackageCache,
    //    indexedPackage: ValidationPackageIndexNew,
    //    ?CacheFolder: string
    //) =

    //    let package = ARCValidationPackageNew.ofPackageIndex(indexedPackage, ?CacheFolder = CacheFolder)
    //    try
    //        match indexedPackage.Metadata.MajorVersion, indexedPackage.Metadata.MinorVersion, indexedPackage.Metadata.PatchVersion with
    //        | 0,0,0 ->
    //            client.GetLatestPackageByNameAsync(indexedPackage.Metadata.Name)
    //            |> Async.AwaitTask
    //            |> Async.RunSynchronously
    //            |> fun script ->
    //                package.Metadata.MajorVersion <- script.MajorVersion
    //                package.Metadata.MinorVersion <- script.MinorVersion
    //                package.Metadata.PatchVersion <- script.PatchVersion
    //                IO.File.WriteAllBytes(indexedPackage.FileName, script.PackageContent)
    //        | _,_,_ ->
    //            client.GetPackageByNameAndVersionAsync(indexedPackage.Metadata.Name,$"{indexedPackage.Metadata.MajorVersion}.{indexedPackage.Metadata.MinorVersion}.{indexedPackage.Metadata.PatchVersion}")
    //            |> Async.AwaitTask
    //            |> Async.RunSynchronously
    //            |> fun script ->
    //                IO.File.WriteAllBytes(indexedPackage.FileName, script.PackageContent)

    //        // cache
    //        // |> PackageCache.addPackage(package)
    //        // |> PackageCache.write()

    //        Ok ($"installed package {package.FileName} at {package.LocalPath}")
    //    with
    //    | e -> Error (DownloadError(package.FileName, e.Message))
    static member SaveAndCachePackage (
        cache: PackageCacheNew,
        packageName: string,
        ?version: string,
        ?CacheFolder: string
    ) =
        try
            let downloadFunctionTask =
                match version with
                | Some v -> client.GetPackageByNameAndVersionAsync(packageName,v)
                | None -> client.GetLatestPackageByNameAsync(packageName)
            let package =
                downloadFunctionTask
                |> Async.AwaitTask
                |> Async.RunSynchronously
                
            let savePath = defaultArg CacheFolder (Defaults.PACKAGE_CACHE_FOLDER())

            let arcValidationPackage =
                ARCValidationPackageNew.create(
                        fileName = package.Name,
                        cacheDate = System.DateTimeOffset.Now,
                        localPath = System.IO.Path.Combine(savePath, package.Name).Replace("\\","/"),
                        metadata = ValidationPackageMetadataNew.create(
                            package.Name,
                            package.Description,
                            package.MajorVersion,
                            package.MinorVersion,
                            package.PatchVersion
                        )
                    )

            cache
            |> PackageCacheNew.addPackage arcValidationPackage
            |> PackageCacheNew.write()
                
            Ok ($"installed package {packageName} at {savePath}")
        with
        | e -> Error (DownloadError(packageName, e.Message))
let t =
    client.GetAllPackagesAsync()
    |> Async.AwaitTask
    |> Async.RunSynchronously
    |> Seq.toArray
//let vpi = 
//    ValidationPackageIndexNew.create(
//        None,
//        @"C:\Users\jonat\Downloads\test.fsx",
//        DateTimeOffset(2024,2,12,0,0,0,TimeSpan.Zero),
//        ValidationPackageMetadataNew.create("test", "this package is here for testing purposes only.")
//    )
//let package = ARCValidationPackageNew.ofPackageIndex vpi

APINew.SaveAndCachePackage(PackageCacheNew(), "test")

type Test () =
    static member stuff (i: int) (?ii: int) =
        i + defaultArg ii 0