module internal TestUtils

open System
open System.IO
open type System.Environment
open ARCValidationPackages

let application_data_path = Environment.GetFolderPath(SpecialFolder.ApplicationData, SpecialFolderOption.Create)

let expected_config_folder_path = Path.Combine(application_data_path, "nfdi4plants/arc-validate").Replace("\\", "/")
let expected_config_file_path = Path.Combine(expected_config_folder_path, "validation-packages-config.json").Replace("\\", "/")

let expected_package_cache_folder_path = Path.Combine(expected_config_folder_path, "package-cache").Replace("\\", "/")
let expected_package_cache_file_path = Path.Combine(expected_package_cache_folder_path, "validation-packages-cache.json").Replace("\\", "/")

let resetConfigEnvironment() =

    // remove any existing config folder for running tests
    if Directory.Exists(expected_config_folder_path) then Directory.Delete(expected_config_folder_path, true)
    if Directory.Exists(expected_package_cache_folder_path) then Directory.Delete(expected_package_cache_folder_path, true)
    // ensure that these file do not exist before running tests
    if File.Exists(expected_config_file_path) then File.Delete(expected_config_file_path)
    if File.Exists(expected_package_cache_file_path) then File.Delete(expected_package_cache_file_path)

let deleteDefaultPackageCache() =
    if File.Exists(expected_package_cache_file_path) then File.Delete(expected_package_cache_file_path)

let deleteDefaultConfig() =
    if File.Exists(expected_config_file_path) then File.Delete(expected_config_file_path)

let get_gh_api_token() = 
    let t = System.Environment.GetEnvironmentVariable("ARC_VALIDATE_GITHUB_API_TEST_TOKEN")
    if isNull(t) then None else Some t

module Result =
    
    let okValue = function
        | Ok value -> value
        | Error error -> failwithf "%A" error

module Fixtures =

    let withFreshConfigAndCache (token:string option) (f: Config * PackageCache -> unit) () =
        resetConfigEnvironment()
        let freshConfig, freshCache = API.GetSyncedConfigAndCache(?Token = token) |> Result.okValue
        f (freshConfig, freshCache)

    //let saveAndCachePackage (token:string option) (package:Package) =
    //    let freshConfig, freshCache = API.GetSyncedConfigAndCache(?Token = token) |> Result.okValue
    //    let updatedCache = API.SaveAndCachePackage(freshConfig, freshCache, package) |> Result.okValue
    //    updatedCache