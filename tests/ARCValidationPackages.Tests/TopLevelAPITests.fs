module TopLevelAPITests

open ARCValidationPackages
open Expecto
open System.IO

open Common.TestUtils
open TestUtils
open ReferenceObjects
open AVPRIndex.Domain

[<Tests>]
let ``Toplevel API tests`` =
    testSequenced (testList "Toplevel API tests" [
        testSequenced (testList "API.Common" [
            test "getSyncedConfigAndCache returns OK" {
                resetConfigEnvironment()
                let syncResult = API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token())
                Expect.isOk (syncResult) "updateIndex did not return OK"
            }
            testSequenced (testList "getSyncedConfigAndCache" [

                yield! testFixture (Fixtures.withFreshConfigAndCaches (get_gh_api_token())) [
                    "Fresh config filepath",
                        fun (freshConfig, _, _) ->
                            Expect.equal freshConfig.ConfigFilePath expected_config_file_path "config file path is not correct"

                    "Fresh package cache preview folder",
                        fun (freshConfig, _, _) ->
                            Expect.equal freshConfig.PackageCacheFolderPreview expected_package_cache_folder_path_preview "package cache preview folder path is not correct"
                
                
                    "Fresh package cache release folder",
                        fun (freshConfig, _, _) ->
                            Expect.equal freshConfig.PackageCacheFolderRelease expected_package_cache_folder_path_release "package cache release folder path is not correct"

                
                    "Fresh config package index contains packages",
                        fun (freshConfig, _, _) ->
                            Expect.isGreaterThan freshConfig.PackageIndex.Length 0 "fresh config package index was empty"

                    "Fresh config package index contains test package",
                        fun (freshConfig, _, _) ->
                            Expect.isTrue (Array.exists (fun (x:ValidationPackageIndex) -> x.Metadata.Name = "test") freshConfig.PackageIndex) "fresh config package index was empty"
                    "Fresh package cache is empty",

                        fun (_, freshCache, _) ->
                            Expect.equal freshCache.Count 0 "fresh cache was not empty"
                ]

            ])
        ])
        testSequenced (testList "API.Preview" [

            testSequenced (testList "updateIndex" [
                yield! testFixture (Fixtures.withFreshConfigAndCaches (get_gh_api_token())) [
                    "updateIndex returns OK",
                        fun (freshConfig, _, _) ->
                            Expect.isOk (API.Preview.UpdateIndex(freshConfig, ?Token = get_gh_api_token())) "updateIndex did not return OK"
                ]   
            ])

            testSequenced (testList "saveAndCachePackage" [
          
            ])
        ])
        testSequenced (testList "API.AVPR" [

            testSequenced (testList "saveAndCachePackage" [
          
            ])
        ])
    ])