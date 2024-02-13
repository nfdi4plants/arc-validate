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
    testSequenced (testList "PackageCache tests" [

        test "createFromPackageList"  {
            Expect.packageCacheEqual 
                (PackageCache([testValidationPackage1]))
                (
                    let tmp = PackageCache()
                    let inner = new Dictionary<string, ARCValidationPackage>()
                    inner["1.0.0"] <- testValidationPackage1
                    tmp["test"] <- inner
                    tmp
                )
        } |> testSequenced

        test "addPackage" {
            Expect.packageCacheEqual 
                (
                    PackageCache()
                    |> PackageCache.addPackage(testValidationPackage1)
                )
                testPackageCache1
        } |> testSequenced

        test "addOfPackageIndex" {
            let actual = 
                PackageCache()
                |> PackageCache.cachePackageOfIndex (testPackageIndex[0], testDate1)
            let expected = testPackageCache1

            Expect.packageCacheEqual actual expected
        }|> testSequenced

        test "CopyViaConstructor" {
            // copy and change the copy, this should not affect the original
            let tmp = PackageCache(testPackageCache1)
            tmp |> PackageCache.removePackage "test" "1.0.0" |> ignore

            let actual = PackageCache([testValidationPackage1]) // this is the initial state of testPackageCache1

            Expect.packageCacheEqual actual testPackageCache1
        } |> testSequenced

        test "updateCacheDate" {
            Expect.packageCacheEqual 
                (
                    PackageCache(testPackageCache1)
                    |> PackageCache.updateCacheDate testValidationPackage1.Metadata.Name (ARCValidationPackage.getSemanticVersionString testValidationPackage1) testDate2
                )
                testPackageCache2
        } |> testSequenced

        test "can write json" {
            deleteDefaultPackageCache() // make sure any cached file is deleted before testing that it can be written
            testPackageCache1 |> PackageCache.write()
            Expect.isTrue (File.Exists(expected_package_cache_file_path)) "package cache file was not created"
        } |> testSequenced

        test "can read json" {
            Expect.packageCacheEqual 
                (PackageCache.read())
                testPackageCache1
        } |> testSequenced

    ])