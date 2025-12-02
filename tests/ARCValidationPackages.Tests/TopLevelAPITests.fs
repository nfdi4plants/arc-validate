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
        testSequenced (testList "Common API" [
            test "GetSyncedConfigAndCache returns OK" {
                resetConfigEnvironment()
                let syncResult = API.Common.GetSyncedConfigAndCache()
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
        testSequenced (testList "AVPR API" [

            testSequenced (testList "SaveAndCachePackage test_3_0_0" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let _, avprCache = Result.okValue (API.Common.GetSyncedConfigAndCache())

                test "SaveAndCachePackage returns OK" {
                    Expect.isOk (API.AVPR.SaveAndCachePackage(avprCache, "test", packageVersion = "3.0.0" )) "SaveAndCachePackage did not return OK"
                }
                test "package is cached after running SaveAndCachePackage" {
                    avprCache 
                    |> Expect.packageCacheContainsPackage "test" "3.0.0"
                }
                test "cached package is correct after running SaveAndCachePackage" {
                    let actual = avprCache |> PackageCache.getPackage "test" "3.0.0"
                    Expect.cachedPackageEqualExceptDate actual CachedValidationPackage.testPackage_3_0_0
                }
                test "package exists after running SaveAndCachePackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path, "test@3.0.0.fsx"))) $"test@3.0.0.fsx did not exist at {expected_package_cache_folder_path}"
                }
            ])

            testSequenced (testList "SaveAndCachePackage test_5_0_0" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let _, avprCache = Result.okValue (API.Common.GetSyncedConfigAndCache())

                test "SaveAndCachePackage returns OK" {
                    Expect.isOk (API.AVPR.SaveAndCachePackage(avprCache, "test", packageVersion = "5.0.0" )) "SaveAndCachePackage did not return OK"
                }
                test "package is cached after running SaveAndCachePackage" {
                    avprCache 
                    |> Expect.packageCacheContainsPackage "test" "5.0.0"
                }
                test "cached package is correct after running SaveAndCachePackage" {
                    let actual = avprCache |> PackageCache.getPackage "test" "5.0.0"
                    Expect.cachedPackageEqualExceptDate actual CachedValidationPackage.testPackage_5_0_0
                }
                test "package exists after running SaveAndCachePackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path, "test@5.0.0.fsx"))) $"test@5.0.0.fsx did not exist at {expected_package_cache_folder_path}"
                }
            ])

            testSequenced (testList "SaveAndCachePackage test_5_0_0-use+suffixes" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let _, avprCache= Result.okValue (API.Common.GetSyncedConfigAndCache())

                test "SaveAndCachePackage returns OK" {
                    Expect.isOk (API.AVPR.SaveAndCachePackage(avprCache, "test", packageVersion = "5.0.0-use+suffixes" )) "SaveAndCachePackage did not return OK"
                }
                test "package is cached after running SaveAndCachePackage" {
                    avprCache 
                    |> Expect.packageCacheContainsPackage "test" "5.0.0-use+suffixes"
                }
                test "cached package is correct after running SaveAndCachePackage" {
                    let actual = avprCache |> PackageCache.getPackage "test" "5.0.0-use+suffixes"
                    Expect.cachedPackageEqualExceptDate actual CachedValidationPackage.``testPackage_5_0_0-use+suffixes``
                }
                test "package exists after running SaveAndCachePackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path, "test@5.0.0-use+suffixes.fsx"))) $"test@5.0.0-use+suffixes.fsx did not exist at {expected_package_cache_folder_path}"
                }
            ])

            testSequenced (testList "InstallPackage" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let _, avprCache= Result.okValue (API.Common.GetSyncedConfigAndCache())

                test "InstallPackage returns OK" {
                    Expect.isOk (API.AVPR.InstallPackage(avprCache, "test", SemVer = "3.0.0")) "InstallPackage did not return OK"
                }
                test "package is cached after running InstallPackage" {
                    avprCache 
                    |> Expect.packageCacheContainsPackage "test" "3.0.0"
                }
                test "package exists after running InstallPackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path, "test@3.0.0.fsx"))) $"test@3.0.0.fsx did not exist at {expected_package_cache_folder_path}"
                }
            ])
            testSequenced (testList "UnInstallPackage" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let config, avprCache = Result.okValue (API.Common.GetSyncedConfigAndCache())

                test "InstallPackage returns OK" {
                    Expect.isOk (API.AVPR.InstallPackage(avprCache, "test", SemVer = "3.0.0")) "InstallPackage did not return OK"
                }
                test "package is cached after running InstallPackage" {
                    avprCache 
                    |> Expect.packageCacheContainsPackage "test" "3.0.0"
                }
                test "package exists after running InstallPackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path, "test@3.0.0.fsx"))) $"test@3.0.0.fsx did not exist at {expected_package_cache_folder_path}"
                }
                test "UninstallPackage with version flag returns OK" {
                    Expect.isOk (API.AVPR.UninstallPackage(avprCache, "test", SemVer = "3.0.0")) "UninstallPackage did not return OK"
                }
                test "package version is not cached anymore after running UninstallPackage" {
                    Expect.isFalse (avprCache["test"].ContainsKey("3.0.0")) "version was not removed from the cache"
                }
                test "package does not exist anymore after running UninstallPackage" {
                    Expect.isFalse (File.Exists (Path.Combine(expected_package_cache_folder_path, "test@3.0.0.fsx"))) $"test@3.0.0.fsx did not exist at {expected_package_cache_folder_path}"
                }
            ])
        ])
    ])