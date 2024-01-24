module ReferenceObjects

open System
open System.IO
open type System.Environment
open ARCValidationPackages

let application_data_path = Environment.GetFolderPath(SpecialFolder.ApplicationData, SpecialFolderOption.Create)

let expected_config_folder_path = Path.Combine(application_data_path, "nfdi4plants/arc-validate").Replace("\\", "/")
let expected_config_file_path = Path.Combine(expected_config_folder_path, "packages-config.json").Replace("\\", "/")

let expected_package_cache_folder_path = Path.Combine(expected_config_folder_path, "arc-validation-packages-cache").Replace("\\", "/")
let expected_package_cache_file_path = Path.Combine(expected_package_cache_folder_path, "packages-cache.json").Replace("\\", "/")

let testDate1 = System.DateTimeOffset.ParseExact("2023-08-15 10:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)
let testDate2 = System.DateTimeOffset.ParseExact("2023-08-15 11:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)


let testPackageIndex = 
    [|
        ValidationPackageIndex.create(
            repoPath = "arc-validate-packages/test.fsx", 
            name = "test",
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
PatchVersion: 0
---
*)

// this file is intended for testing purposes only.
printfn \"Hello, world!\"".ReplaceLineEndings()

let testValidationPackage1 =
    ARCValidationPackage.create(
        "test",
        testDate1,
        (Path.Combine(expected_package_cache_folder_path, "test.fsx").Replace("\\","/")),
        ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", 1, 0, 0)
    )

let testValidationPackage2 =
    ARCValidationPackage.create(
        "test",
        testDate2,
        (Path.Combine(expected_package_cache_folder_path, "test.fsx").Replace("\\","/")),
        ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", 1, 0, 0)
    )

let testPackageCache1 = PackageCache([testValidationPackage1.Name, testValidationPackage1])
let testPackageCache2 = PackageCache([testValidationPackage1.Name, testValidationPackage2])

let testScriptPath = "fixtures/testScript.fsx"
let testScriptArgsPath = "fixtures/testScriptArgs.fsx"

let testScriptPackage = ARCValidationPackage.create("testScript", testDate1, testScriptPath, ValidationPackageMetadata())
let testScriptArgsPackage = ARCValidationPackage.create("testScriptArgs", testDate1, testScriptArgsPath, ValidationPackageMetadata())