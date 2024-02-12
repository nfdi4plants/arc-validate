module PackageCacheTests

open Expecto
open ARCValidationPackages
open System.IO
open ReferenceObjects
open TestUtils
open Common.TestUtils
open TestUtils

[<Tests>]
let tests =
    testSequenced (testList "PackageCache tests" [

        test "addPackage" {
            Expect.sequenceEqual 
                (
                    PackageCache()
                    |> PackageCache.addPackage(testValidationPackage1)
                )
                testPackageCache1
                "ARCValidationPackage was not added to the PackageCache correctly."
        } |> testSequenced
        testList "addOfPackageIndex" [
            let actual = 
                PackageCache()
                |> PackageCache.cachePackageOfIndex (testPackageIndex[0], testDate1)
            let expected = testPackageCache1

            for key in expected.Keys do
                yield test key {Expect.sequenceEqual actual.[key] expected.[key] "ARCValidationPackage was not added to the PackageCache correctly."}
        ]|> testSequenced

        testList "CopyViaConstructor" [
            // copy and change the copy, this should not affect the original
            let tmp = PackageCache(testPackageCache1)
            tmp |> PackageCache.removePackage testValidationPackage1.Metadata.Name (ARCValidationPackage.getSemanticVersionString testValidationPackage1) |> ignore

            let actual = PackageCache([testValidationPackage1]) // this is the initial state of testPackageCache1

            for key in actual.Keys do
                yield test key {Expect.sequenceEqual actual.[key] testPackageCache1.[key] "PackageCache was not copied correctly."}
        ] |> testSequenced

        test "updateCacheDate" {
            Expect.sequenceEqual 
                (
                    PackageCache(testPackageCache1)
                    |> PackageCache.updateCacheDate testValidationPackage1.Metadata.Name (ARCValidationPackage.getSemanticVersionString testValidationPackage1) testDate2
                )
                testPackageCache2
                "ARCValidationPackagecache date in PackageCache was not updated correctly."
        } |> testSequenced

        test "can write json" {
            deleteDefaultPackageCache() // make sure any cached file is deleted before testing that it can be written
            testPackageCache1 |> PackageCache.write()
            Expect.isTrue (File.Exists(expected_package_cache_file_path)) "package cache file was not created"
        } |> testSequenced

        test "can read json" {
            Expect.sequenceEqual 
                (PackageCache.read())
                testPackageCache1
                "package cache file was not read correctly"
        } |> testSequenced

    ])