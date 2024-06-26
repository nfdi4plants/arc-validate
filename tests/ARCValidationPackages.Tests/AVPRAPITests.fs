﻿module AVPRAPITests

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
        testList "GetPackageByNameAndVersion" [
            test "test_3_0_0" {
                let vp = AVPR.api.GetPackageByNameAndVersion "test" "3.0.0"
                Expect.AVPRClient.validationPackageEqual vp ReferenceObjects.AVPRClientDomain.ValidationPackage.testPackage_3_0_0
            }
            test "test_5_0_0 - CQCHookEndpoint addition" {
                let vp = AVPR.api.GetPackageByNameAndVersion "test" "5.0.0"
                Expect.AVPRClient.validationPackageEqual vp ReferenceObjects.AVPRClientDomain.ValidationPackage.testPackage_5_0_0
            }
            test "test_5_0_0-use+suffixes - SemVer addition" {
                let vp = AVPR.api.GetPackageByNameAndVersion "test" "5.0.0-use+suffixes"
                Expect.AVPRClient.validationPackageEqual vp ReferenceObjects.AVPRClientDomain.ValidationPackage.``testPackage_5_0_0-use+suffixes``
            }
        
        ]
        test "GetAllPackages contains a test package" {
            let indexedPackages = AVPR.api.GetAllPackages()
            Expect.isTrue (indexedPackages |> Array.exists (fun package -> package.Name = "test")) "package index did not contain test script"
        }
        testList "downloadPackageScript" [
            test "test_3_0_0" {
                Expect.equal 
                    (
                        AVPR.api.downloadPackageScript( "test", "3.0.0")
                        |> fun content -> content.ReplaceLineEndings("\n")
                    )
                    ReferenceObjects.testScriptContentAVPR_test_3_0_0
                    "script content was not correct"
            }
            test "test_5_0_0 - CQCHookEndpoint addition" {
                Expect.equal 
                    (
                        AVPR.api.downloadPackageScript( "test", "5.0.0")
                        |> fun content -> content.ReplaceLineEndings("\n")
                    )
                    ReferenceObjects.testScriptContentAVPR_test_5_0_0
                    "script content was not correct"
            }
            test "test_5_0_0-use+suffixes - SemVer addition" {
                Expect.equal 
                    (
                        AVPR.api.downloadPackageScript( "test", "5.0.0-use+suffixes")
                        |> fun content -> content.ReplaceLineEndings("\n")
                    )
                    ReferenceObjects.``testScriptContentAVPR_test_5_0_0-use+suffixes``
                    "script content was not correct"
            }
        ]
    ]