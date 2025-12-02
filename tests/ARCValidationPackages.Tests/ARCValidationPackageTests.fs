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
                    CachedValidationPackage.testValidationPackage1
                    |> CachedValidationPackage.updateCacheDate testDate2
                )
                CachedValidationPackage.testValidationPackage2
                "ARCValidationPackage cache date was not updated correctly."
        }
    ]
