﻿namespace ARCValidationPackages

open FsHttp
open System
open System.Text
open System.Text.Json

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
            GET $"{Defaults.GITHUB_API_BASE_URL}/repos/{owner}/{repo}/contents/{path}"
            UserAgent Defaults.GITHUB_API_USER_AGENT
            Accept Defaults.GITHUB_API_ACCEPT_HEADER
            headers [
                if Token.IsSome then "Authorization", $"Bearer {Token.Value}"
            ]
        }
        |> Request.send
        |> Response.toJson

    static member getPackageIndex (?Token: string) =
        GitHubAPI.getRepositoryContent(
            owner = Defaults.PACKAGE_INDEX_OWNER,
            repo = Defaults.PACKAGE_INDEX_REPO,
            path = Defaults.PACKAGE_INDEX_FILE_NAME,
            userAgent = Defaults.GITHUB_API_USER_AGENT,
            ?Token = Token
        )
        |> fun json -> (json?content).GetString()
        |> fun content -> Convert.FromBase64String(content)
        |> fun bytes -> Encoding.UTF8.GetString(bytes)
        |> fun index -> JsonSerializer.Deserialize<ValidationPackageIndex[]>(index, Defaults.SERIALIZATION_OPTIONS)

    static member downloadPackageScript (packagePath: string, ?Token: string) =
        GitHubAPI.getRepositoryContent(
            owner = Defaults.PACKAGE_INDEX_OWNER,
            repo = Defaults.PACKAGE_INDEX_REPO,
            path = packagePath,
            userAgent = Defaults.GITHUB_API_USER_AGENT,
            ?Token = Token
        )
        |> fun json -> (json?content).GetString()
        |> fun content -> Convert.FromBase64String(content)
        |> fun bytes -> Encoding.UTF8.GetString(bytes)