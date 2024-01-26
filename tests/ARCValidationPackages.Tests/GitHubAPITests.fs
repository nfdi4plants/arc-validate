module GitHubAPITests

open Expecto
open ARCValidationPackages
open FsHttp
open System
open System.Text

[<Tests>]
let ``GitHubAPI tests`` = 
    testList "GitHubAPI tests" [
        test "getRepositoryContent terminates" {
            GitHubAPI.getRepositoryContent(
                owner = "nfdi4plants",
                repo = "arc-validate-packages",
                path = "README.md",
                userAgent = "arc-validate-test"
            )
            |> ignore
        }
        test "getRepositoryContent returns correct content" {
            Expect.equal 
                (
                    GitHubAPI.getRepositoryContent(
                        owner = "nfdi4plants",
                        repo = "arc-validate-packages",
                        path = "arc-validate-packages/test.fsx",
                        userAgent = "arc-validate-test"
                    )
                    |> fun json -> (json?content).GetString()
                    |> fun content -> Convert.FromBase64String(content)
                    |> fun bytes -> Encoding.UTF8.GetString(bytes).ReplaceLineEndings()
                )
                ReferenceObjects.testScriptContent
                "repository content was not correct"
        }
        test "getPackageIndex terminates" {
            GitHubAPI.getPackageIndex()
            |> ignore
        }
        test "getPackageIndex contains test script" {
            let indexedPackages = GitHubAPI.getPackageIndex()
            Expect.isTrue (indexedPackages |> Array.exists (fun package -> package.RepoPath = "arc-validate-packages/test.fsx")) "package index did not contain test script"
        }
        test "getScriptContent terminates" {
            GitHubAPI.downloadPackageScript("arc-validate-packages/test.fsx")
            |> ignore
        }
        test "getScriptContent returns correct content" {
            Expect.equal 
                (
                    GitHubAPI.downloadPackageScript("arc-validate-packages/test.fsx")
                    |> fun content -> content.ReplaceLineEndings()
                )
                ReferenceObjects.testScriptContent
                "script content was not correct"
        }
    ]