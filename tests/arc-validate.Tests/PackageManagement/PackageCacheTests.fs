module PackageCacheTests

open Expecto
open ARCValidate.PackageManagement
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
                (PackageCache([CachedValidationPackage.testValidationPackage1]))
                (
                    let tmp = PackageCache()
                    let inner = new Dictionary<string, CachedValidationPackage>()
                    inner["1.0.0"] <- CachedValidationPackage.testValidationPackage1
                    tmp["test"] <- inner
                    tmp
                )
        } |> testSequenced

        test "addPackage" {
            Expect.packageCacheEqual 
                (
                    PackageCache()
                    |> PackageCache.addPackage(CachedValidationPackage.testValidationPackage1)
                )
                PackageCache.testPackageCache1
        } |> testSequenced

        test "CopyViaConstructor" {
            // copy and change the copy, this should not affect the original
            let tmp = PackageCache(PackageCache.testPackageCache1)
            tmp |> PackageCache.removePackage "test" "1.0.0" |> ignore

            let actual = PackageCache([CachedValidationPackage.testValidationPackage1]) // this is the initial state of testPackageCache1

            Expect.packageCacheEqual actual PackageCache.testPackageCache1
        } |> testSequenced

        test "updateCacheDate" {
            Expect.packageCacheEqual 
                (
                    PackageCache(PackageCache.testPackageCache1)
                    |> PackageCache.updateCacheDate CachedValidationPackage.testValidationPackage1.Metadata.Name (CachedValidationPackage.getSemanticVersionString CachedValidationPackage.testValidationPackage1) testDate2
                )
                PackageCache.testPackageCache2
        } |> testSequenced

        test "can write json" {
            //deleteDefaultPackageCache() // make sure any cached file is deleted before testing that it can be written
            PackageCache.testPackageCache1 |> PackageCache.write(Defaults.PACKAGE_CACHE_FOLDER())
            Expect.isTrue (File.Exists(expected_package_cache_file_path)) "package cache file was not created"
        } |> testSequenced

        test "can read json" {
            Expect.packageCacheEqual 
                (PackageCache.read(Defaults.PACKAGE_CACHE_FILE_PATH()))
                PackageCache.testPackageCache1
        } |> testSequenced

        test "getLatestPackage returns latest stable package with no suffixes" {
            let actual = 
                PackageCache([
                    CachedValidationPackage.``testPackage_5_0_0-use+suffixes``
                    CachedValidationPackage.testPackage_5_0_0
                    CachedValidationPackage.testPackage_3_0_0
                ]) |> PackageCache.getLatestPackage "test"
            Expect.cachedPackageEqualExceptDate actual CachedValidationPackage.testPackage_5_0_0
        } |> testSequenced

        test "getLatestPackage fails when no package is cached" {
            Expect.throws (fun () -> PackageCache() |> PackageCache.getLatestPackage "test" |> ignore) "getLatestPackage did not fail"
        } |> testSequenced

        test "tryGetLatestPackage returns some latest stable package with no suffixes" {
            let actual = 
                PackageCache([
                    CachedValidationPackage.``testPackage_5_0_0-use+suffixes``
                    CachedValidationPackage.testPackage_5_0_0
                    CachedValidationPackage.testPackage_3_0_0
                ]) |> PackageCache.tryGetLatestPackage "test"
            Expect.isSome actual ""
            Expect.cachedPackageEqualExceptDate actual.Value CachedValidationPackage.testPackage_5_0_0
        } |> testSequenced

        test "tryGetLatestPackage returns None when no package is cached" {
            Expect.isNone (PackageCache() |> PackageCache.tryGetLatestPackage "test") "tryGetLatestPackage was not None"
        } |> testSequenced

    ])
