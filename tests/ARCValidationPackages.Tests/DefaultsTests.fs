module DefaultsTests

open Expecto
open ARCValidationPackages
open System
open System.IO
open type System.Environment

let application_data_path = Environment.GetFolderPath(SpecialFolder.ApplicationData, SpecialFolderOption.Create)

let expected_config_folder_path = Path.Combine(application_data_path, "nfdi4plants/arc-validate").Replace("\\", "/")
let expected_package_cache_folder_path = Path.Combine(expected_config_folder_path, "arc-validation-packages-cache").Replace("\\", "/")
let expected_config_file_path = Path.Combine(expected_config_folder_path, "arc-validation-packages.config").Replace("\\", "/")

// remove any existing config folder for running tests
if Directory.Exists(expected_config_folder_path) then Directory.Delete(expected_config_folder_path, true)

[<Tests>]
let ``Defaults tests`` =
    testSequenced (
        testList "Defaults tests" [
            test "config folder path is correct" {
                Expect.equal 
                    (Defaults.CONFIG_FOLDER()) 
                    expected_config_folder_path
                    "config folder path is not correct"
            }
            test "config folder path exists" {
                Expect.isTrue 
                    (Directory.Exists(expected_config_folder_path))
                    "config folder path does not exist"
            }
            test "package cache folder path is correct" {
                Expect.equal 
                    (Defaults.PACKAGE_CACHE_FOLDER()) 
                    expected_package_cache_folder_path
                    "package cache folder path is not correct"
            }
            test "package cache folder path exists" {
                Expect.isTrue 
                    (Directory.Exists(expected_package_cache_folder_path))
                    "package cache folder path does not exist"
            }
            test "config file path is correct" {
                Expect.equal 
                    (Defaults.CONFIG_FILE_PATH()) 
                    expected_config_file_path
                    "config file path is not correct"
            }
        ]
    )