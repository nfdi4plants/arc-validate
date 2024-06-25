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
                let syncResult = API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token())
                Expect.isOk (syncResult) "GetSyncedConfigAndCache did not return OK"
            }
            testSequenced (testList "GetSyncedConfigAndCache" [

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
            testSequenced (testList "ListCachedPackages" [

            ])
        ])
        testSequenced (testList "Preview API" [

            testSequenced (testList "UpdateIndex" [
                yield! testFixture (Fixtures.withFreshConfigAndCaches (get_gh_api_token())) [
                    "UpdateIndex returns OK",
                        fun (freshConfig, _, _) ->
                            Expect.isOk (API.Preview.UpdateIndex(freshConfig, ?Token = get_gh_api_token())) "UpdateIndex did not return OK"
                ]
            ])
            testSequenced (testList "ListIndexedPackages" [

                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let config, _, _ = Result.okValue (API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token()))
                
                // we need a package index to be able to list indexed packages
                test "UpdateIndex returns OK" {
                    Expect.isOk (API.Preview.UpdateIndex(config, ?Token = get_gh_api_token())) "UpdateIndex did not return OK"
                }
                test "ListIndexedPackages returns OK" {
                    Expect.isOk (API.Preview.ListIndexedPackages(config)) "ListIndexedPackages did not return OK"
                }

            ])
            testSequenced (testList "SaveAndCachePackage" [
                
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let config, _, previewCache = Result.okValue (API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token()))
                let firstIndexedPackage = config.PackageIndex.[0]
                let firstPackageName = firstIndexedPackage.Metadata.Name
                let firstPackageVersion = ValidationPackageMetadata.getSemanticVersionString firstIndexedPackage.Metadata

                test "SaveAndCachePackage returns OK" {
                    Expect.isOk (API.Preview.SaveAndCachePackage(previewCache, firstIndexedPackage)) "SaveAndCachePackage did not return OK"
                }
                test "package is cached after running SaveAndCachePackage" {
                    previewCache 
                    |> Expect.packageCacheContainsPackage firstPackageName firstPackageVersion
                }
                test "package exists after running SaveAndCachePackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path_preview, $"{firstPackageName}@{firstPackageVersion}.fsx"))) $"{firstPackageName}@{firstPackageVersion}.fsx did not exist at {expected_package_cache_folder_path_preview}"
                }
            ])
            testSequenced (testList "InstallPackage" [
                
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let config, _, previewCache = Result.okValue (API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token()))
                let firstIndexedPackage = config.PackageIndex.[0]
                let firstPackageName = firstIndexedPackage.Metadata.Name
                let firstPackageVersion = ValidationPackageMetadata.getSemanticVersionString firstIndexedPackage.Metadata

                test "InstallPackage returns OK" {
                    Expect.isOk (API.Preview.InstallPackage(config, previewCache, firstPackageName, SemVer = firstPackageVersion)) "InstallPackage did not return OK"
                }
                test "package is cached after running InstallPackage" {
                    previewCache 
                    |> Expect.packageCacheContainsPackage firstPackageName firstPackageVersion
                }
                test "package exists after running InstallPackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path_preview, $"{firstPackageName}@{firstPackageVersion}.fsx"))) $"{firstPackageName}@{firstPackageVersion}.fsx did not exist at {expected_package_cache_folder_path_preview}"
                }
            ])
            testSequenced (testList "UninstallPackage" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let config, _, previewCache = Result.okValue (API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token()))
                let firstIndexedPackage = config.PackageIndex.[0]
                let firstPackageName = firstIndexedPackage.Metadata.Name
                let firstPackageVersion = ValidationPackageMetadata.getSemanticVersionString firstIndexedPackage.Metadata

                test "InstallPackage returns OK" {
                    Expect.isOk (API.Preview.InstallPackage(config, previewCache, firstPackageName, SemVer = firstPackageVersion)) "InstallPackage did not return OK"
                }
                test "package is cached after running InstallPackage" {
                    previewCache 
                    |> Expect.packageCacheContainsPackage firstPackageName firstPackageVersion
                }
                test "package exists after running InstallPackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path_preview, $"{firstPackageName}@{firstPackageVersion}.fsx"))) $"{firstPackageName}@{firstPackageVersion}.fsx did not exist at {expected_package_cache_folder_path_preview}"
                }
                test "UninstallPackage with version flag returns OK" {
                    Expect.isOk (API.Preview.UninstallPackage(previewCache, firstPackageName, SemVer = firstPackageVersion)) "UninstallPackage did not return OK"
                }
                test "package version is not cached anymore after running UninstallPackage" {
                    Expect.isFalse (previewCache[firstPackageName].ContainsKey(firstPackageVersion)) "version was not removed from the cache"
                }
                test "package does not exist anymore after running UninstallPackage" {
                    Expect.isFalse (File.Exists (Path.Combine(expected_package_cache_folder_path_preview, $"{firstPackageName}@{firstPackageVersion}.fsx"))) $"{firstPackageName}@{firstPackageVersion}.fsx did not exist at {expected_package_cache_folder_path_preview}"
                }
            ])
        ])
        testSequenced (testList "AVPR API" [

            testSequenced (testList "SaveAndCachePackage test_3_0_0" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let _, avprCache, _ = Result.okValue (API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token()))

                test "SaveAndCachePackage returns OK" {
                    Expect.isOk (API.AVPR.SaveAndCachePackage(avprCache, "test", packageVersion = "3.0.0" )) "SaveAndCachePackage did not return OK"
                }
                test "package is cached after running SaveAndCachePackage" {
                    avprCache 
                    |> Expect.packageCacheContainsPackage "test" "3.0.0"
                }
                test "cached package is correct after running SaveAndCachePackage" {
                    let actual = avprCache |> PackageCache.getPackage "test" "3.0.0"
                    Expect.cachedPackageEqualExceptDate actual CachedValidationPackage.AVPR.testPackage_3_0_0
                }
                test "package exists after running SaveAndCachePackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path_release, "test@3.0.0.fsx"))) $"test@3.0.0.fsx did not exist at {expected_package_cache_folder_path_release}"
                }
            ])

            testSequenced (testList "SaveAndCachePackage test_5_0_0" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let _, avprCache, _ = Result.okValue (API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token()))

                test "SaveAndCachePackage returns OK" {
                    Expect.isOk (API.AVPR.SaveAndCachePackage(avprCache, "test", packageVersion = "5.0.0" )) "SaveAndCachePackage did not return OK"
                }
                test "package is cached after running SaveAndCachePackage" {
                    avprCache 
                    |> Expect.packageCacheContainsPackage "test" "5.0.0"
                }
                test "cached package is correct after running SaveAndCachePackage" {
                    let actual = avprCache |> PackageCache.getPackage "test" "5.0.0"
                    Expect.cachedPackageEqualExceptDate actual CachedValidationPackage.AVPR.testPackage_5_0_0
                }
                test "package exists after running SaveAndCachePackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0.fsx"))) $"test@5.0.0.fsx did not exist at {expected_package_cache_folder_path_release}"
                }
            ])

            testSequenced (testList "SaveAndCachePackage test_5_0_0-use+suffixes" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let _, avprCache, _ = Result.okValue (API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token()))

                test "SaveAndCachePackage returns OK" {
                    Expect.isOk (API.AVPR.SaveAndCachePackage(avprCache, "test", packageVersion = "5.0.0-use+suffixes" )) "SaveAndCachePackage did not return OK"
                }
                test "package is cached after running SaveAndCachePackage" {
                    avprCache 
                    |> Expect.packageCacheContainsPackage "test" "5.0.0-use+suffixes"
                }
                test "cached package is correct after running SaveAndCachePackage" {
                    let actual = avprCache |> PackageCache.getPackage "test" "5.0.0-use+suffixes"
                    Expect.cachedPackageEqualExceptDate actual CachedValidationPackage.AVPR.``testPackage_5_0_0-use+suffixes``
                }
                test "package exists after running SaveAndCachePackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path_release, "test@5.0.0-use+suffixes.fsx"))) $"test@5.0.0-use+suffixes.fsx did not exist at {expected_package_cache_folder_path_release}"
                }
            ])

            testSequenced (testList "InstallPackage" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let _, avprCache, _ = Result.okValue (API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token()))

                test "InstallPackage returns OK" {
                    Expect.isOk (API.AVPR.InstallPackage(avprCache, "test", SemVer = "3.0.0")) "InstallPackage did not return OK"
                }
                test "package is cached after running InstallPackage" {
                    avprCache 
                    |> Expect.packageCacheContainsPackage "test" "3.0.0"
                }
                test "package exists after running InstallPackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path_release, "test@3.0.0.fsx"))) $"test@3.0.0.fsx did not exist at {expected_package_cache_folder_path_release}"
                }
            ])
            testSequenced (testList "UnInstallPackage" [
                // here, wee need persistent config and caches across the tests instead of a fixture for each test case
                resetConfigEnvironment()
                let config, avprCache, _ = Result.okValue (API.Common.GetSyncedConfigAndCache(?Token = get_gh_api_token()))

                test "InstallPackage returns OK" {
                    Expect.isOk (API.Preview.InstallPackage(config, avprCache, "test", SemVer = "3.0.0")) "InstallPackage did not return OK"
                }
                test "package is cached after running InstallPackage" {
                    avprCache 
                    |> Expect.packageCacheContainsPackage "test" "3.0.0"
                }
                test "package exists after running InstallPackage" {
                    Expect.isTrue (File.Exists (Path.Combine(expected_package_cache_folder_path_release, "test@3.0.0.fsx"))) $"test@3.0.0.fsx did not exist at {expected_package_cache_folder_path_release}"
                }
                test "UninstallPackage with version flag returns OK" {
                    Expect.isOk (API.Preview.UninstallPackage(avprCache, "test", SemVer = "3.0.0")) "UninstallPackage did not return OK"
                }
                test "package version is not cached anymore after running UninstallPackage" {
                    Expect.isFalse (avprCache["test"].ContainsKey("3.0.0")) "version was not removed from the cache"
                }
                test "package does not exist anymore after running UninstallPackage" {
                    Expect.isFalse (File.Exists (Path.Combine(expected_package_cache_folder_path_preview, "test@3.0.0.fsx"))) $"test@3.0.0.fsx did not exist at {expected_package_cache_folder_path_release}"
                }
            ])
        ])
    ])