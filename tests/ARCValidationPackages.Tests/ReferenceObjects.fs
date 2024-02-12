module ReferenceObjects

open System
open System.IO
open type System.Environment
open ARCValidationPackages
open Common.TestUtils
open TestUtils


let testDate1 = System.DateTimeOffset.ParseExact("2023-08-15 10:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)
let testDate2 = System.DateTimeOffset.ParseExact("2023-08-15 11:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)


let testPackageIndex = 
    [|
        ValidationPackageIndex.create(
            repoPath = "arc-validate-packages/test.fsx", 
            fileName = "test",
            lastUpdated = testDate1,
            metadata = ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", 1, 0, 0)
        )
    |]

let testScriptContent = "(*
---
Name: test
Description: this package is here for testing purposes only.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 1
---
*)

// this file is intended for testing purposes only.
printfn \"Hello, world!\"".ReplaceLineEndings()

let testValidationPackage1 =
    ARCValidationPackage.create(
        "test@1.0.0.fsx",
        testDate1,
        (Path.Combine(expected_package_cache_folder_path, "test.fsx").Replace("\\","/")),
        ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", 1, 0, 0)
    )

let testValidationPackage2 =
    ARCValidationPackage.create(
        "test@1.0.0.fsx",
        testDate2,
        (Path.Combine(expected_package_cache_folder_path, "test.fsx").Replace("\\","/")),
        ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", 1, 0, 0)
    )

let testPackageCache1 = PackageCache([testValidationPackage1])
let testPackageCache2 = PackageCache([testValidationPackage2])

let testScriptPath = "fixtures/testScript.fsx"
let testScriptArgsPath = "fixtures/testScriptArgs.fsx"

let testScriptPackage = ARCValidationPackage.create("testScript", testDate1, testScriptPath, ValidationPackageMetadata())
let testScriptArgsPackage = ARCValidationPackage.create("testScriptArgs", testDate1, testScriptArgsPath, ValidationPackageMetadata())