module ARCValidationPackageTests

open Expecto
open ARCValidationPackages
open ReferenceObjects

[<Tests>]
let tests =
    testList "ARCValidationPackage tests" [
        test "updateCacheDate" {
             Expect.equal 
                (
                    testValidationPackage1
                    |> ARCValidationPackage.updateCacheDate testDate2
                )
                testValidationPackage2
                "ARCValidationPackage cache date was not updated correctly."
        }
        test "ofPackageIndex" {
            Expect.equal 
                (
                    ARCValidationPackage.ofPackageIndex(
                        packageIndex = testPackageIndex[0],
                        Date = testDate2
                    )
                )
                testValidationPackage2
                "ARCValidationPackage was not created correctly."
        }
    ]
