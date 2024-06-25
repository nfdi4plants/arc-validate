module PackageCacheTests

open Expecto
open ARCValidationPackages
open System.IO
open ReferenceObjects
open TestUtils
open Common.TestUtils
open TestUtils
open System.Collections.Generic

[<Tests>]
let tests =
    testSequenced (testList "Domain Tests.PackageCache tests" [

        test "createFromPackageList"  {
            Expect.packageCacheEqual 
                (PackageCache([CachedValidationPackage.Preview.testValidationPackage1]))
                (
                    let tmp = PackageCache()
                    let inner = new Dictionary<string, CachedValidationPackage>()
                    inner["1.0.0"] <- CachedValidationPackage.Preview.testValidationPackage1
                    tmp["test"] <- inner
                    tmp
                )
        } |> testSequenced

        test "addPackage" {
            Expect.packageCacheEqual 
                (
                    PackageCache()
                    |> PackageCache.addPackage(CachedValidationPackage.Preview.testValidationPackage1)
                )
                PackageCache.Preview.testPackageCache1
        } |> testSequenced

        test "addOfPackageIndex" {
            let actual = 
                PackageCache()
                |> PackageCache.cachePackageOfIndex (testPackageIndex[0], testDate1)
            let expected = PackageCache.Preview.testPackageCache1

            Expect.packageCacheEqual actual expected
        }|> testSequenced

        test "CopyViaConstructor" {
            // copy and change the copy, this should not affect the original
            let tmp = PackageCache(PackageCache.Preview.testPackageCache1)
            tmp |> PackageCache.removePackage "test" "1.0.0" |> ignore

            let actual = PackageCache([CachedValidationPackage.Preview.testValidationPackage1]) // this is the initial state of testPackageCache1

            Expect.packageCacheEqual actual PackageCache.Preview.testPackageCache1
        } |> testSequenced

        test "updateCacheDate" {
            Expect.packageCacheEqual 
                (
                    PackageCache(PackageCache.Preview.testPackageCache1)
                    |> PackageCache.updateCacheDate CachedValidationPackage.Preview.testValidationPackage1.Metadata.Name (CachedValidationPackage.getSemanticVersionString CachedValidationPackage.Preview.testValidationPackage1) testDate2
                )
                PackageCache.Preview.testPackageCache2
        } |> testSequenced

        test "can write json preview" {
            deleteDefaultPackageCache() // make sure any cached file is deleted before testing that it can be written
            PackageCache.Preview.testPackageCache1 |> PackageCache.write(Defaults.PACKAGE_CACHE_FOLDER_PREVIEW())
            Expect.isTrue (File.Exists(expected_package_cache_file_path_preview)) "package cache file was not created"
        } |> testSequenced

        test "can write json release" {
            //deleteDefaultPackageCache() // make sure any cached file is deleted before testing that it can be written
            PackageCache.Preview.testPackageCache1 |> PackageCache.write(Defaults.PACKAGE_CACHE_FOLDER_RELEASE())
            Expect.isTrue (File.Exists(expected_package_cache_file_path_release)) "package cache file was not created"
        } |> testSequenced

        test "can read json preview" {
            Expect.packageCacheEqual 
                (PackageCache.read(Defaults.PACKAGE_CACHE_FILE_PATH_PREVIEW()))
                PackageCache.Preview.testPackageCache1
        } |> testSequenced

        test "can read json release" {
            Expect.packageCacheEqual 
                (PackageCache.read(Defaults.PACKAGE_CACHE_FILE_PATH_RELEASE()))
                PackageCache.Preview.testPackageCache1
        } |> testSequenced

        test "getLatestPackage returns latest stable package with no suffixes" {
            let actual = 
                PackageCache([
                    CachedValidationPackage.Preview.``testPackage_5_0_0-use+suffixes``
                    CachedValidationPackage.Preview.testPackage_5_0_0
                    CachedValidationPackage.Preview.testPackage_3_0_0
                ]) |> PackageCache.getLatestPackage "test"
            Expect.cachedPackageEqualExceptDate actual CachedValidationPackage.Preview.testPackage_5_0_0
        } |> testSequenced

        test "getLatestPackage fails when no package is cached" {
            Expect.throws (fun () -> PackageCache() |> PackageCache.getLatestPackage "test" |> ignore) "getLatestPackage did not fail"
        } |> testSequenced

        test "tryGetLatestPackage returns some latest stable package with no suffixes" {
            let actual = 
                PackageCache([
                    CachedValidationPackage.Preview.``testPackage_5_0_0-use+suffixes``
                    CachedValidationPackage.Preview.testPackage_5_0_0
                    CachedValidationPackage.Preview.testPackage_3_0_0
                ]) |> PackageCache.tryGetLatestPackage "test"
            Expect.isSome actual ""
            Expect.cachedPackageEqualExceptDate actual.Value CachedValidationPackage.Preview.testPackage_5_0_0
        } |> testSequenced

        test "tryGetLatestPackage returns None when no package is cached" {
            Expect.isNone (PackageCache() |> PackageCache.tryGetLatestPackage "test") "tryGetLatestPackage was not None"
        } |> testSequenced

    ])