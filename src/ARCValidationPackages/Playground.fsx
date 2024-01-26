#r "nuget: Fake.DotNet.Cli, 6.0.0"
#r "nuget: FsHttp, 11.0.0"

#load "Results.fs"
#load "Defaults.fs"
#load "Domain.fs"
#load "GitHubAPI.fs"
#load "PackageCache.fs"
#load "Config.fs"
#load "ScriptExecution.fs"
#load "TopLevelAPI.fs"

open ARCValidationPackages

let token = 
    let t = System.Environment.GetEnvironmentVariable("ARC_VALIDATE_GITHUB_API_TEST_TOKEN")
    if isNull(t) then None else Some t

let configPath = $"C:/Users/schne/Desktop/lol/{Defaults.CONFIG_FILE_NAME}"
let cacheFolder = $@"C:/Users/schne/Desktop/lol/cache"

let syncResult = API.GetSyncedConfigAndCache(ConfigPath = configPath, CacheFolder = cacheFolder, ?Token = token)

let freshConfigAPI, freshCacheAPI =
    match syncResult with
    | Result.Ok (config, cache) -> config, cache
    | Result.Error e -> failwithf "sync error %A" e

let anotherIndex = GitHubAPI.getPackageIndex(?Token = token)

let updatedConfig = 
    API.UpdateIndex(freshConfigAPI, ?Token = token)

let saveResults = 
    API.SaveAndCachePackage(
        cache = freshCacheAPI,
        CacheFolder= freshConfigAPI.PackageCacheFolder,
        indexedPackage = (Array.find (fun p -> p.FileName = "test") freshConfigAPI.PackageIndex),
        ?Token = token
    )