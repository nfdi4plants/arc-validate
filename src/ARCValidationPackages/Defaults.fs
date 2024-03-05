namespace ARCValidationPackages

open System.Text.Json
open System.IO
open type System.Environment

module Defaults =

    let [<Literal>] PACKAGE_INDEX_URL = "https://github.com/nfdi4plants/arc-validate-package-registry"

    let [<Literal>] PACKAGE_INDEX_OWNER = "nfdi4plants"

    let [<Literal>] PACKAGE_INDEX_REPO = "arc-validate-package-registry"

    let [<Literal>] PACKAGE_INDEX_FILE_NAME = "src/PackageRegistryService/Data/arc-validate-package-index.json"

    let [<Literal>] PACKAGE_STAGING_AREA = "src/PackageRegistryService/StagingArea"

    let [<Literal>] GITHUB_API_BASE_URL = "https://api.github.com"

    let [<Literal>] GITHUB_API_USER_AGENT = "arc-validate"

    let [<Literal>] GITHUB_API_ACCEPT_HEADER = "application/vnd.github+json"

    let [<Literal>] CONFIG_FILE_NAME = "validation-packages-config.json"

    let [<Literal>] PACKAGE_CACHE_FOLDER_NAME_PREVIEW = "package-cache-preview"

    let [<Literal>] PACKAGE_CACHE_FOLDER_NAME_RELEASE = "package-cache-release"

    let [<Literal>] PACKAGE_CACHE_FILE_NAME = "validation-packages-cache.json"

    let CONFIG_FOLDER() = 
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

    let PACKAGE_CACHE_FOLDER_PREVIEW () = 
        let path = 
            Path.Combine(CONFIG_FOLDER(), PACKAGE_CACHE_FOLDER_NAME_PREVIEW)
                .Replace("\\", "/")
        Directory.CreateDirectory(path) |> ignore
        path

    let PACKAGE_CACHE_FOLDER_RELEASE () = 
        let path = 
            Path.Combine(CONFIG_FOLDER(), PACKAGE_CACHE_FOLDER_NAME_RELEASE)
                .Replace("\\", "/")
        Directory.CreateDirectory(path) |> ignore
        path

    let PACKAGE_CACHE_FILE_PATH_PREVIEW () = 
        Path.Combine(PACKAGE_CACHE_FOLDER_PREVIEW(), PACKAGE_CACHE_FILE_NAME)
            .Replace("\\", "/")

    let PACKAGE_CACHE_FILE_PATH_RELEASE () = 
        Path.Combine(PACKAGE_CACHE_FOLDER_RELEASE(), PACKAGE_CACHE_FILE_NAME)
            .Replace("\\", "/")

    let SERIALIZATION_OPTIONS =  JsonSerializerOptions(WriteIndented = true)

