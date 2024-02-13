namespace ARCValidationPackages

open FsHttp
open System
open System.IO
open System.Text
open System.Text.Json

module GitHubAPI =

    module Errors =

        type RateLimitError(msg : string) =
            inherit Exception(msg)

        type SerializationError(msg : string) =
            inherit Exception(msg)

        type BadCredentialsError(msg : string) =
            inherit Exception(msg)

        type NotFoundError(msg : string) =
            inherit Exception(msg)

type GitHubAPI =
    static member getRepositoryContent(
        owner: string,
        repo: string,
        path: string,
        userAgent: string,
        ?Token: string
    ) = 
        let url = $"{Defaults.GITHUB_API_BASE_URL}/repos/{owner}/{repo}/contents/{path}"
        let response = 
            http {
                GET url
                UserAgent Defaults.GITHUB_API_USER_AGENT
                Accept Defaults.GITHUB_API_ACCEPT_HEADER
                headers [ 
                    if Token.IsSome then "Authorization", $"Bearer {Token.Value}"
                ]
            }
            |> Request.send

        if response.reasonPhrase = "rate limit exceeded" && response.statusCode = Net.HttpStatusCode.Forbidden then
            raise (GitHubAPI.Errors.RateLimitError($"{response.reasonPhrase}:{System.Environment.NewLine}at: {url}{System.Environment.NewLine}{response |> Response.toJson}"))

        elif response.statusCode = Net.HttpStatusCode.Unauthorized then
            raise (GitHubAPI.Errors.BadCredentialsError($"{response.reasonPhrase}:{System.Environment.NewLine}at: {url}{System.Environment.NewLine}{response |> Response.toJson}"))

        elif response.statusCode = Net.HttpStatusCode.NotFound then
            raise (GitHubAPI.Errors.NotFoundError($"{response.reasonPhrase}:{System.Environment.NewLine}at: {url}{System.Environment.NewLine}{response |> Response.toJson}"))

        else 
            try
                response
                |> Response.toJson

            with e ->
                raise (GitHubAPI.Errors.SerializationError(e.Message))



    static member getPackageIndex (?Token: string) =
        let json = 
            GitHubAPI.getRepositoryContent(
                owner = Defaults.PACKAGE_INDEX_OWNER,
                repo = Defaults.PACKAGE_INDEX_REPO,
                path = Defaults.PACKAGE_INDEX_FILE_NAME,
                userAgent = Defaults.GITHUB_API_USER_AGENT,
                ?Token = Token
            )
        try
            json
            |> fun json -> (json?content).GetString()
            |> fun content -> Convert.FromBase64String(content)
            |> fun bytes -> Encoding.UTF8.GetString(bytes)
            |> fun index -> JsonSerializer.Deserialize<ValidationPackageIndex[]>(index, Defaults.SERIALIZATION_OPTIONS)
        with e ->
            raise (GitHubAPI.Errors.SerializationError($"{e.Message}{System.Environment.NewLine}{json}"))

    static member downloadPackageScript (packagePath: string, ?Token: string) =
        let json = 
            GitHubAPI.getRepositoryContent(
                owner = Defaults.PACKAGE_INDEX_OWNER,
                repo = Defaults.PACKAGE_INDEX_REPO,
                path = packagePath,
                userAgent = Defaults.GITHUB_API_USER_AGENT,
                ?Token = Token
            )
        try
            json
            |> fun json -> (json?content).GetString()
            |> fun content -> Convert.FromBase64String(content)
            |> fun bytes -> Encoding.UTF8.GetString(bytes)
        with e ->
            raise (GitHubAPI.Errors.SerializationError($"{e.Message}{System.Environment.NewLine}{json}"))

    static member downloadPackageScript (packageIndex: ValidationPackageIndex, ?Token: string) =
        GitHubAPI.downloadPackageScript(packageIndex.RepoPath, ?Token = Token)