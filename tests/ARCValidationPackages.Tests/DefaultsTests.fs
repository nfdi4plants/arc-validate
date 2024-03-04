﻿module DefaultsTests

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
            test "package cache folder path is correct" {
                Expect.equal 
                    (Defaults.PACKAGE_CACHE_FOLDER_PREVIEW()) 
                    expected_package_cache_folder_path_preview
                    "package cache folder path is not correct"
            }
            test "package cache folder path exists" {
                Expect.isTrue 
                    (Directory.Exists(expected_package_cache_folder_path_preview))
                    "package cache folder path does not exist"
            }
            test "package cache file path is correct" {
                Expect.equal 
                    (Defaults.PACKAGE_CACHE_FILE_PATH_PREVIEW()) 
                    expected_package_cache_file_path_preview
                    "config file path is not correct"
            }

        ]
    )