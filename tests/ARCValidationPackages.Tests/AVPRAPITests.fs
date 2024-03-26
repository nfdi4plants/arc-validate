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
let ``AVPRAPI tests`` = 
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
        test "getScriptContent returns correct content" {
            Expect.equal 
                (
                    AVPR.api.downloadPackageScript( "test", "3.0.0")
                    |> fun content -> content.ReplaceLineEndings()
                )
                ReferenceObjects.testScriptContentAVPR
                "script content was not correct"
        }
    ]