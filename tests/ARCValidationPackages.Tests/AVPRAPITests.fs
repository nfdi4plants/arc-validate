module AVPRAPITests

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
    testList "AVPR API tests" [
        test "getRepositoryContent returns correct content" {
            let vp = AVPR.api.GetPackageByNameAndVersion "test" "3.0.0"
            let script =
                vp.PackageContent
                |> Encoding.UTF8.GetString
            Expect.equal 
                (script.ReplaceLineEndings())
                ReferenceObjects.testScriptContentAVPR
                $"repository content was not correct{System.Environment.NewLine}{script}"
        }
        test "getPackageIndex contains test script" {
            let indexedPackages = AVPR.api.GetAllPackages()
            Expect.isTrue (indexedPackages |> Array.exists (fun package -> package.Name = "test")) "package index did not contain test script"
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
                    |> fun content -> content.ReplaceLineEndings()
                )
                ReferenceObjects.testScriptContent
                "script content was not correct"
        }
    ]