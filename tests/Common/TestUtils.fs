namespace Common

open System
open System.IO
open type System.Environment
open Fake.Core

module ErrorMessage =
    let withProcessDiagnostics (message: string) proc (tool:string) (args: seq<string>) =
        $"""{message}{System.Environment.NewLine}O: {proc.Result.Output}E: {System.Environment.NewLine}{proc.Result.Error}{System.Environment.NewLine}(tool: {tool} args: {args |> String.concat " "})"""
    
    let withCLIDiagnostics (message: string) (tool:string) (args: seq<string>) =
        $"""{message}{System.Environment.NewLine}(tool: {tool} args: {args |> String.concat " "})"""


module TestUtils =
 
    let application_data_path = Environment.GetFolderPath(SpecialFolder.ApplicationData, SpecialFolderOption.Create)

    let expected_config_folder_path = Path.Combine(application_data_path, "nfdi4plants/arc-validate").Replace("\\", "/")
    let expected_config_file_path = Path.Combine(expected_config_folder_path, "validation-packages-config.json").Replace("\\", "/")

    let expected_package_cache_folder_path_preview = Path.Combine(expected_config_folder_path, "package-cache-preview").Replace("\\", "/")
    let expected_package_cache_file_path_preview = Path.Combine(expected_package_cache_folder_path_preview, "validation-packages-cache.json").Replace("\\", "/")

    let expected_package_cache_folder_path_release = Path.Combine(expected_config_folder_path, "package-cache-release").Replace("\\", "/")
    let expected_package_cache_file_path_release = Path.Combine(expected_package_cache_folder_path_release, "validation-packages-cache.json").Replace("\\", "/")


    let runTool (tool: string) (args: string []) =
        CreateProcess.fromRawCommand tool args
        |> CreateProcess.redirectOutput
        |> Proc.run

    let resetConfigEnvironment() =

        // remove any existing config folder for running tests
        if Directory.Exists(expected_config_folder_path) then Directory.Delete(expected_config_folder_path, true)
        if Directory.Exists(expected_package_cache_folder_path_preview) then Directory.Delete(expected_package_cache_folder_path_preview, true)
        if Directory.Exists(expected_package_cache_folder_path_release) then Directory.Delete(expected_package_cache_folder_path_release, true)
        // ensure that these file do not exist before running tests
        if File.Exists(expected_config_file_path) then File.Delete(expected_config_file_path)
        if File.Exists(expected_package_cache_file_path_preview) then File.Delete(expected_package_cache_file_path_preview)
        if File.Exists(expected_package_cache_file_path_release) then File.Delete(expected_package_cache_file_path_release)

    let deleteDefaultPackageCache() =
        if File.Exists(expected_package_cache_file_path_preview) then File.Delete(expected_package_cache_file_path_preview)
        if File.Exists(expected_package_cache_file_path_release) then File.Delete(expected_package_cache_file_path_release)

    let deleteDefaultConfig() =
        if File.Exists(expected_config_file_path) then File.Delete(expected_config_file_path)

    let get_gh_api_token() = 
        let t = System.Environment.GetEnvironmentVariable("ARC_VALIDATE_GITHUB_API_TEST_TOKEN")
        if isNull(t) then None else Some t

    module Result =
    
        let okValue = function
            | Ok value -> value
            | Error error -> failwithf "%A" error