// For more information see https://aka.ms/fsharp-console-apps
open ARCValidationPackages

open ARCValidationPackages

let token = 
    let t = System.Environment.GetEnvironmentVariable("ARC_VALIDATE_GITHUB_API_TEST_TOKEN")
    if isNull(t) then None else Some t

let configPath = @"C:/Users/schne/Desktop/lol/test.json"
let cacheFolder = @"C:/Users/schne/Desktop/lol/cache"

let syncResult = API.GetSyncedConfigAndCache(false, ConfigPath = configPath, CacheFolderPreview = cacheFolder, ?Token = token)

printfn "%A" syncResult