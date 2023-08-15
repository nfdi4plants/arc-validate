namespace ARCValidationPackages

open System.Text.Json
open System.IO
open type System.Environment

module Defaults =

    let [<Literal>] PACKAGE_INDEX_URL = "https://github.com/nfdi4plants/arc-validate-packages"

    let [<Literal>] PACKAGE_INDEX_OWNER = "nfdi4plants"

    let [<Literal>] PACKAGE_INDEX_REPO = "arc-validate-packages"

    let [<Literal>] PACKAGE_INDEX_FILE_NAME = "arc-validate-package-index.json"

    let [<Literal>] GITHUB_API_BASE_URL = "https://api.github.com"

    let [<Literal>] GITHUB_API_USER_AGENT = "arc-validate"

    let [<Literal>] GITHUB_API_ACCEPT_HEADER = "application/vnd.github+json"

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
        Path.Combine(CONFIG_FOLDER(), "packages-config.json")
            .Replace("\\", "/")

    let PACKAGE_CACHE_FOLDER () = 
        let path = 
            Path.Combine(CONFIG_FOLDER(), "arc-validation-packages-cache")
                .Replace("\\", "/")
        Directory.CreateDirectory(path) |> ignore
        path

    let PACKAGE_CACHE_FILE_PATH () = 
        Path.Combine(PACKAGE_CACHE_FOLDER(), "packages-cache.json")
            .Replace("\\", "/")

    let SERIALIZATION_OPTIONS =  JsonSerializerOptions(WriteIndented = true)

