module PackageCacheTests

open Expecto
open ARCValidationPackages
open ReferenceObjects

[<Tests>]
let tests =
    testList "PackageCache tests" [
        test "addPackage" {
            Expect.sequenceEqual 
                (
                    PackageCache()
                    |> PackageCache.addPackage(testValidationPackage1)
                )
                testPackageCache1
                "ARCValidationPackage was not added to the PackageCache correctly."
        }
        test "addOfPackageIndex" {
            Expect.sequenceEqual 
                (
                    PackageCache()
                    |> PackageCache.addOfPackageIndex(testPackageIndex[0], testDate1)
                )
                testPackageCache1
                "ARCValidationPackage was not added to the PackageCache correctly."
        }
        test "CopyViaConstructor" {
            // copy and change the copy, this should not affect the original
            let tmp = PackageCache(testPackageCache1)
            tmp |> PackageCache.removePackage testValidationPackage1.Name |> ignore

            Expect.sequenceEqual 
                (PackageCache([testValidationPackage1.Name, testValidationPackage1])) // this is the initial state of testPackageCache1
                testPackageCache1
                "PackageCache was not copied correctly."
        }

        test "updateCacheDate" {
            Expect.sequenceEqual 
                (
                    PackageCache(testPackageCache1)
                    |> PackageCache.updateCacheDate testValidationPackage1.Name testDate2
                )
                testPackageCache2
                "ARCValidationPackagecache date in PackageCache was not updated correctly."
        }
    ]