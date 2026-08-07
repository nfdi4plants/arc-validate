module TopLevelAPITests

open ARCValidate.PackageManagement
open Expecto
open System.IO

open Common.TestUtils
open TestUtils
open ReferenceObjects
open ValidationPackage.Model

[<Tests>]
let ``Toplevel API tests`` =
    testSequenced (testList "Toplevel API tests" [
        testSequenced (testList "Common API" [
            test "GetSyncedConfigAndCache returns OK" {
                resetConfigEnvironment()
                let syncResult = Common.GetSyncedConfigAndCache()
                Expect.isOk (syncResult) "GetSyncedConfigAndCache did not return OK"
            }
            testSequenced (testList "GetSyncedConfigAndCache" [

                yield! testFixture (Fixtures.withFreshConfigAndCache) [
                    "Fresh config filepath",
                        fun (freshConfig, _) ->
                            Expect.equal freshConfig.ConfigFilePath expected_config_file_path "config file path is not correct"

                    "Fresh package cache folder",
                        fun (freshConfig, _) ->
                            Expect.equal freshConfig.PackageCacheFolder expected_package_cache_folder_path "package cache release folder path is not correct"

                    "Fresh package cache is empty",
                        fun (_, freshCache) ->
                            Expect.equal freshCache.Count 0 "fresh cache was not empty"
                ]

            ])
            testSequenced (testList "ListCachedPackages" [

            ])
        ])
    ])
