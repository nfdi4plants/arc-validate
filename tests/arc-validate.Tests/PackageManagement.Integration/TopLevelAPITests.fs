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
        testSequenced (testList "AVPR API" [

            testSequenced (testList "SaveAndCachePackage test_3_0_0" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let _, avprCache = Result.okValue (Common.GetSyncedConfigAndCache())

                test "SaveAndCachePackage returns OK" {
                    let result =
                        ARCValidate.PackageManagement.AVPR.SaveAndCachePackageAsync(
                            avprCache,
                            "test",
                            PackageVersion = "3.0.0",
                            RegistryApi = AVPR.api
                        )
                        |> AVPR.await

                    Expect.isOk result "SaveAndCachePackage did not return OK"
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
                let _, avprCache = Result.okValue (Common.GetSyncedConfigAndCache())

                test "SaveAndCachePackage returns OK" {
                    let result =
                        ARCValidate.PackageManagement.AVPR.SaveAndCachePackageAsync(
                            avprCache,
                            "test",
                            PackageVersion = "5.0.0",
                            RegistryApi = AVPR.api
                        )
                        |> AVPR.await

                    Expect.isOk result "SaveAndCachePackage did not return OK"
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
                let _, avprCache= Result.okValue (Common.GetSyncedConfigAndCache())

                test "SaveAndCachePackage returns OK" {
                    let result =
                        ARCValidate.PackageManagement.AVPR.SaveAndCachePackageAsync(
                            avprCache,
                            "test",
                            PackageVersion = "5.0.0-use+suffixes",
                            RegistryApi = AVPR.api
                        )
                        |> AVPR.await

                    Expect.isOk result "SaveAndCachePackage did not return OK"
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
                let _, avprCache= Result.okValue (Common.GetSyncedConfigAndCache())

                test "InstallPackage returns OK" {
                    let result =
                        ARCValidate.PackageManagement.AVPR.InstallPackageAsync(
                            avprCache,
                            "test",
                            SemVer = "3.0.0",
                            RegistryApi = AVPR.api
                        )
                        |> AVPR.await

                    Expect.isOk result "InstallPackage did not return OK"
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
                let config, avprCache = Result.okValue (Common.GetSyncedConfigAndCache())

                test "InstallPackage returns OK" {
                    let result =
                        ARCValidate.PackageManagement.AVPR.InstallPackageAsync(
                            avprCache,
                            "test",
                            SemVer = "3.0.0",
                            RegistryApi = AVPR.api
                        )
                        |> AVPR.await

                    Expect.isOk result "InstallPackage did not return OK"
                }
                test "package is cached after running InstallPackage" {
                    avprCache
                    |> Expect.packageCacheContainsPackage "test" "3.0.0"
                }
                test "package exists after running InstallPackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path, "test@3.0.0.fsx"))) $"test@3.0.0.fsx did not exist at {expected_package_cache_folder_path}"
                }
                test "UninstallPackage with version flag returns OK" {
                    Expect.isOk
                        (ARCValidate.PackageManagement.AVPR.UninstallPackage(
                            avprCache,
                            "test",
                            SemVer = "3.0.0",
                            CacheFolder = config.PackageCacheFolder
                        ))
                        "UninstallPackage did not return OK"
                }
                test "package version is not cached anymore after running UninstallPackage" {
                    Expect.isFalse (avprCache.ContainsKey("test")) "package was not removed from the cache"
                }
                test "package does not exist anymore after running UninstallPackage" {
                    Expect.isFalse (File.Exists (Path.Combine(expected_package_cache_folder_path, "test@3.0.0.fsx"))) $"test@3.0.0.fsx did not exist at {expected_package_cache_folder_path}"
                }
            ])
        ])
    ])
