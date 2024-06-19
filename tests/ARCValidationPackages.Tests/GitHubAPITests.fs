module GitHubAPITests

open Expecto
open ARCValidationPackages
open FsHttp
open System
open System.Text
open Common.TestUtils
open TestUtils

let token = get_gh_api_token()

[<Tests>]
let ``GitHubAPI tests`` = 
    testList "GitHubAPI tests" [
        test "getRepositoryContent terminates" {
            GitHubAPI.getRepositoryContent(
                owner = "nfdi4plants",
                repo = "arc-validate-package-registry",
                path = "README.md",
                userAgent = "arc-validate-test",
                ?Token = token
            )
            |> ignore
        }
        test "getRepositoryContent returns correct content" {
            let response = 
                GitHubAPI.getRepositoryContent(
                    owner = "nfdi4plants",
                    repo = "arc-validate-package-registry",
                    path = "StagingArea/test/test@1.0.0.fsx",
                    userAgent = "arc-validate-test",
                    ?Token = token
                )
            Expect.equal 
                (
                    response
                    |> fun json -> (json?content).GetString()
                    |> fun content -> Convert.FromBase64String(content)
                    |> fun bytes -> Encoding.UTF8.GetString(bytes).ReplaceLineEndings("\n")
                )
                ReferenceObjects.testScriptContent
                $"repository content was not correct{System.Environment.NewLine}{response}"
        }
        test "getPackageIndex terminates" {
            GitHubAPI.getPackageIndex(?Token = token)
            |> ignore
        }
        test "getPackageIndex contains test script" {
            let indexedPackages = GitHubAPI.getPackageIndex(?Token = token)
            Expect.isTrue (indexedPackages |> Array.exists (fun package -> package.RepoPath = "./StagingArea/test/test@1.0.0.fsx")) "package index did not contain test script"
        }
        test "getScriptContent terminates" {
            GitHubAPI.downloadPackageScript(
                "StagingArea/test/test@1.0.0.fsx",
                ?Token = token
            )
            |> ignore
        }
        test "getScriptContent returns correct content" {
            Expect.equal 
                (
                    GitHubAPI.downloadPackageScript(
                        "StagingArea/test/test@1.0.0.fsx",
                        ?Token = token
                    )
                    |> fun content -> content.ReplaceLineEndings("\n")
                )
                ReferenceObjects.testScriptContent
                "script content was not correct"
        }
    ]