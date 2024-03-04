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

        test "getSyncedConfigAndCache returns OK" {
            resetConfigEnvironment()
            let syncResult = API.GetSyncedConfigAndCache(?Token = get_gh_api_token())
            Expect.isOk (syncResult) "updateIndex did not return OK"
        }

        testSequenced (testList "getSyncedConfigAndCache" [

            yield! testFixture (Fixtures.withFreshConfigAndCache (get_gh_api_token())) [
                "Fresh config filepath",
                    fun (freshConfig, _) ->
                        Expect.equal freshConfig.ConfigFilePath expected_config_file_path "config file path is not correct"

                "Fresh package cache folder",
                    fun (freshConfig, _) ->
                        Expect.equal freshConfig.PackageCacheFolder expected_package_cache_folder_path "package cache folder path is not correct"

                "Fresh config package index contains packages",
                    fun (freshConfig, _) ->
                        Expect.isGreaterThan freshConfig.PackageIndex.Length 0 "fresh config package index was empty"

                "Fresh config package index contains test package",
                    fun (freshConfig, _) ->
                        Expect.isTrue (Array.exists (fun (x:ValidationPackageIndex) -> x.Metadata.Name = "test") freshConfig.PackageIndex) "fresh config package index was empty"
                "Fresh package cache is empty",

                    fun (_, freshCache) ->
                        Expect.equal freshCache.Count 0 "fresh cache was not empty"
            ]

        ])

        testSequenced (testList "updateIndex" [
            yield! testFixture (Fixtures.withFreshConfigAndCache (get_gh_api_token())) [
                "updateIndex returns OK",
                    fun (freshConfig, _) ->
                        Expect.isOk (API.UpdateIndex(freshConfig, ?Token = get_gh_api_token())) "updateIndex did not return OK"
            ]   
        ])

        testSequenced (testList "saveAndCachePackage" [
          
        ])
    ])