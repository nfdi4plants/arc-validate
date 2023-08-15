module ReferenceObjects

open System
open System.IO
open type System.Environment
open ARCValidationPackages

let application_data_path = Environment.GetFolderPath(SpecialFolder.ApplicationData, SpecialFolderOption.Create)

let expected_config_folder_path = Path.Combine(application_data_path, "nfdi4plants/arc-validate").Replace("\\", "/")
let expected_package_cache_folder_path = Path.Combine(expected_config_folder_path, "arc-validation-packages-cache").Replace("\\", "/")
let expected_config_file_path = Path.Combine(expected_config_folder_path, "packages-config.json").Replace("\\", "/")

// remove any existing config folder for running tests
if Directory.Exists(expected_config_folder_path) then Directory.Delete(expected_config_folder_path, true)
// ensure that the config file does not exist before running tests
if File.Exists(expected_config_file_path) then File.Delete(expected_config_file_path)

let testDate = System.DateTimeOffset.ParseExact("2023-08-15 10:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)

let testPackageIndex = 
    [|
        ValidationPackageIndex.create(
            "arc-validate-packages/test.fsx", testDate
        )
    |]

let testConfig =
    Config.create(
        testPackageIndex,
        testDate,
        Defaults.PACKAGE_CACHE_FOLDER(),
        Defaults.CONFIG_FILE_PATH()
    )

let testScriptContent = "// this file is intended for testing purposes only.
printfn \"Hello, world!\"".ReplaceLineEndings()
