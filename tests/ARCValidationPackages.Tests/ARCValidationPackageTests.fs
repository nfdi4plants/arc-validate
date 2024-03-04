module ARCValidationPackageTests

open Expecto
open ARCValidationPackages
open ReferenceObjects
open Common.TestUtils
open TestUtils

[<Tests>]
let tests =
    testList "ARCValidationPackage tests" [
        test "updateCacheDate" {
             Expect.equal 
                (
                    testValidationPackage1
                    |> CachedValidationPackage.updateCacheDate testDate2
                )
                testValidationPackage2
                "ARCValidationPackage cache date was not updated correctly."
        }
        test "ofPackageIndex" {
            Expect.equal 
                (
                    CachedValidationPackage.ofPackageIndex(
                        packageIndex = testPackageIndex[0],
                        Date = testDate2
                    )
                )
                testValidationPackage2
                "ARCValidationPackage was not created correctly."
        }
    ]
