namespace ARCValidationPackages

open System.Text.Json
open type System.Environment

module Defaults =

    let [<Literal>] PACKAGE_INDEX_URL = "https://github.com/nfdi4plants/arc-validate-packages"

    let [<Literal>] PACKAGE_INDEX_OWNER = "nfdi4plants"

    let [<Literal>] PACKAGE_INDEX_REPO = "arc-validate-packages"

    let [<Literal>] PACKAGE_INDEX_FILE_NAME = "arc-validate-package-index.json"

    let [<Literal>] GITHUB_API_BASE_URL = "https://api.github.com"

    let [<Literal>] GITHUB_API_USER_AGENT = "arc-validate"

    let [<Literal>] GITHUB_API_ACCEPT_HEADER = "application/vnd.github+json"

    let PACKAGE_CACHE_LOCATION () = 
        GetFolderPath(
            SpecialFolder.ApplicationData,
            SpecialFolderOption.Create
        )

    let SERIALIZATION_OPTIONS =  JsonSerializerOptions(WriteIndented = true)

