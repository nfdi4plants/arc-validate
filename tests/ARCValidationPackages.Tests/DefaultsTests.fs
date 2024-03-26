module DefaultsTests

open Expecto
open ARCValidationPackages
open System
open System.IO
open type System.Environment

open ReferenceObjects
open Common.TestUtils
open TestUtils

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
            test "config file path is correct" {
                Expect.equal 
                    (Defaults.CONFIG_FILE_PATH()) 
                    expected_config_file_path
                    "config file path is not correct"
            }
            test "package cache preview folder path is correct" {
                Expect.equal 
                    (Defaults.PACKAGE_CACHE_FOLDER_PREVIEW()) 
                    expected_package_cache_folder_path_preview
                    "package cache preview folder path is not correct"
            }
            test "package cache release folder path is correct" {
                Expect.equal 
                    (Defaults.PACKAGE_CACHE_FOLDER_RELEASE()) 
                    expected_package_cache_folder_path_release
                    "package cache release folder path is not correct"
            }
            test "package cache preview folder path exists" {
                Expect.isTrue 
                    (Directory.Exists(expected_package_cache_folder_path_preview))
                    "package cache preview folder path does not exist"
            }
            test "package cache release folder path exists" {
                Expect.isTrue 
                    (Directory.Exists(expected_package_cache_folder_path_release))
                    "package cache release folder path does not exist"
            }
            test "package cache preview file path is correct" {
                Expect.equal 
                    (Defaults.PACKAGE_CACHE_FILE_PATH_PREVIEW()) 
                    expected_package_cache_file_path_preview
                    "config file preview path is not correct"
            }
            test "package cache release file path is correct" {
                Expect.equal 
                    (Defaults.PACKAGE_CACHE_FILE_PATH_RELEASE()) 
                    expected_package_cache_file_path_release
                    "config file release path is not correct"
            }
        ]
    )