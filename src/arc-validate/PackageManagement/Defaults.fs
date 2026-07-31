namespace ARCValidate.PackageManagement

open System.IO
open System.Text.Json
open type System.Environment

module Defaults =

    let [<Literal>] CONFIG_FILE_NAME = "validation-packages-config.json"

    let [<Literal>] PACKAGE_CACHE_FOLDER_NAME = "package-cache-release"

    let [<Literal>] PACKAGE_CACHE_FILE_NAME = "validation-packages-cache.json"

    let [<Literal>] REGISTRY_API_BASE_URL = "https://avpr.nfdi4plants.org"

    let [<Literal>] REGISTRY_API_BASE_URL_ENVIRONMENT_VARIABLE = "ARC_VALIDATE_AVPR_URL"

    let CONFIG_FOLDER () =
        let path =
            GetFolderPath(
                SpecialFolder.ApplicationData,
                SpecialFolderOption.Create
            )
            |> fun path ->
                Path.Combine(path, "nfdi4plants/arc-validate")
                    .Replace("\\", "/")

        Directory.CreateDirectory(path) |> ignore
        path

    let CONFIG_FILE_PATH () =
        Path.Combine(CONFIG_FOLDER(), CONFIG_FILE_NAME)
            .Replace("\\", "/")

    let PACKAGE_CACHE_FOLDER () =
        let path =
            Path.Combine(CONFIG_FOLDER(), PACKAGE_CACHE_FOLDER_NAME)
                .Replace("\\", "/")

        Directory.CreateDirectory(path) |> ignore
        path

    let PACKAGE_CACHE_FILE_PATH () =
        Path.Combine(PACKAGE_CACHE_FOLDER(), PACKAGE_CACHE_FILE_NAME)
            .Replace("\\", "/")

    let REGISTRY_API_URL () =
        match GetEnvironmentVariable(REGISTRY_API_BASE_URL_ENVIRONMENT_VARIABLE) with
        | value when System.String.IsNullOrWhiteSpace(value) -> REGISTRY_API_BASE_URL
        | value -> value.TrimEnd('/')

    let SERIALIZATION_OPTIONS = JsonSerializerOptions(WriteIndented = true)
