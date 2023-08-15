module ReferenceObjects

open System
open System.IO
open type System.Environment
open ARCValidationPackages

let application_data_path = Environment.GetFolderPath(SpecialFolder.ApplicationData, SpecialFolderOption.Create)

let expected_config_folder_path = Path.Combine(application_data_path, "nfdi4plants/arc-validate").Replace("\\", "/")
let expected_package_cache_folder_path = Path.Combine(expected_config_folder_path, "arc-validation-packages-cache").Replace("\\", "/")
let expected_package_cache_file_path = Path.Combine(expected_package_cache_folder_path, "packages-cache.json").Replace("\\", "/")
let expected_config_file_path = Path.Combine(expected_config_folder_path, "packages-config.json").Replace("\\", "/")

// remove any existing config folder for running tests
if Directory.Exists(expected_config_folder_path) then Directory.Delete(expected_config_folder_path, true)
if Directory.Exists(expected_package_cache_folder_path) then Directory.Delete(expected_package_cache_folder_path, true)
// ensure that these file do not exist before running tests
if File.Exists(expected_config_file_path) then File.Delete(expected_config_file_path)
if File.Exists(expected_package_cache_file_path) then File.Delete(expected_package_cache_file_path)

let testDate1 = System.DateTimeOffset.ParseExact("2023-08-15 10:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)
let testDate2 = System.DateTimeOffset.ParseExact("2023-08-15 11:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)

let testPackageIndex = 
    [|
        ValidationPackageIndex.create(
            repoPath = "arc-validate-packages/test.fsx", 
            name = "test",
            lastUpdated = testDate1
        )
    |]

let testConfig =
    Config.create(
        testPackageIndex,
        testDate1,
        Defaults.PACKAGE_CACHE_FOLDER(),
        Defaults.CONFIG_FILE_PATH()
    )

let testScriptContent = "// this file is intended for testing purposes only.
printfn \"Hello, world!\"".ReplaceLineEndings()

let testValidationPackage1 =
    ARCValidationPackage.create(
        "test",
        testDate1,
        (Path.Combine(Defaults.PACKAGE_CACHE_FOLDER(), "test.fsx").Replace("\\","/"))
    )

let testValidationPackage2 =
    ARCValidationPackage.create(
        "test",
        testDate2,
        (Path.Combine(Defaults.PACKAGE_CACHE_FOLDER(), "test.fsx").Replace("\\","/"))
    )

let testPackageCache1 = PackageCache([testValidationPackage1.Name, testValidationPackage1])
let testPackageCache2 = PackageCache([testValidationPackage1.Name, testValidationPackage2])

let testScriptPath = "fixtures/testScript.fsx"
let testScriptArgsPath = "fixtures/testScriptArgs.fsx"

let testScriptPackage = ARCValidationPackage.create("testScript", testDate1, testScriptPath)
let testScriptArgsPackage = ARCValidationPackage.create("testScriptArgs", testDate1, testScriptArgsPath)