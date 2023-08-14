#r "nuget: FsHttp, 11.0.0"

open FsHttp
open System
open System.Text
open System.Text.Json

let [<Literal>] PACKAGE_INDEX_OWNER = "nfdi4plants"

let [<Literal>] PACKAGE_INDEX_REPO = "arc-validate-packages"

let [<Literal>] PACKAGE_INDEX_FILE_NAME = "arc-validate-package-index.json"

let [<Literal>] GITHUB_API_BASE_URL = "https://api.github.com"

let [<Literal>] GITHUB_API_USER_AGENT = "arc-validate"

let [<Literal>] GITHUB_API_ACCEPT_HEADER = "application/vnd.github+json"

http {
    GET $"{GITHUB_API_BASE_URL}/repos/{PACKAGE_INDEX_OWNER}/{PACKAGE_INDEX_REPO}/contents/{PACKAGE_INDEX_FILE_NAME}"
    UserAgent GITHUB_API_USER_AGENT
    Accept GITHUB_API_ACCEPT_HEADER
}
|> Request.send
|> Response.toJson

type ValidationPackageIndex =
    {
        Name: string
        LastUpdated: System.DateTimeOffset
    } with
        static member create (name: string, lastUpdated: System.DateTimeOffset) = { Name = name; LastUpdated = lastUpdated }

type GitHubAPI =
    static member getRepositoryContent(
        owner: string,
        repo: string,
        path: string,
        userAgent: string,
        ?Token: string
    ) = 
        http {
            GET $"{GITHUB_API_BASE_URL}/repos/{owner}/{repo}/contents/{path}"
            UserAgent GITHUB_API_USER_AGENT
            Accept GITHUB_API_ACCEPT_HEADER
            headers [
                if Token.IsSome then "Authorization", $"Bearer {Token.Value}"
            ]
        }
        |> Request.send
        |> Response.toJson

    static member getPackageIndex (?Token: string) =
        GitHubAPI.getRepositoryContent(
            owner = PACKAGE_INDEX_OWNER,
            repo = PACKAGE_INDEX_REPO,
            path = PACKAGE_INDEX_FILE_NAME,
            userAgent = GITHUB_API_USER_AGENT,
            ?Token = Token
        )
        |> fun json -> (json?content).GetString()
        |> fun content -> Convert.FromBase64String(content)
        |> fun bytes -> Encoding.UTF8.GetString(bytes)
        |> fun index -> JsonSerializer.Deserialize<ValidationPackageIndex[]>(index)


GitHubAPI.getPackageIndex()
