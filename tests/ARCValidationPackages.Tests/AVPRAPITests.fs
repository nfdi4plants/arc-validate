module AVPRAPITests

open Expecto
open ARCValidationPackages
open FsHttp
open System
open System.Text
open Common.TestUtils
open TestUtils
open AVPRClient.Interop
open ValidationPackage.Model

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
            test "test_7_0_0 exposes CWL command inputs from the development registry" {
                let vp = AVPR.api.GetPackageByNameAndVersion "test" "7.0.0"
                let clientInput = vp.Inputs |> Seq.find (fun input -> input.Id = "test")

                Expect.equal vp.Inputs.Count 2 "CWL input count was not correct"
                Expect.equal clientInput.Type AVPRClient.CommandInputType.Boolean_ "CWL scalar type was not correct"
                Expect.equal clientInput.Doc "Enable test mode" "CWL documentation was not correct"
                Expect.equal clientInput.InputBinding.Position 0 "CWL argument position was not correct"
                Expect.equal clientInput.InputBinding.Prefix "--test" "CWL argument prefix was not correct"
                Expect.isTrue clientInput.InputBinding.Separate "CWL separate flag was not correct"

                let modelInput =
                    vp.ToModel().Inputs
                    |> Array.find (fun input -> input.Id = "test")

                Expect.equal modelInput.Type.PrimitiveType CwlPrimitive.Boolean "Portable primitive type was not correct"
                Expect.isTrue modelInput.Type.IsNullable "Portable CWL nullability was not correct"
                Expect.equal modelInput.InputBinding.Prefix "--test" "Portable argument prefix was not correct"
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
