module internal TestUtils

open System
open System.IO
open type System.Environment

open ReferenceObjects

let resetConfigEnvironment() =

    // remove any existing config folder for running tests
    if Directory.Exists(expected_config_folder_path) then Directory.Delete(expected_config_folder_path, true)
    if Directory.Exists(expected_package_cache_folder_path) then Directory.Delete(expected_package_cache_folder_path, true)
    // ensure that these file do not exist before running tests
    if File.Exists(expected_config_file_path) then File.Delete(expected_config_file_path)
    if File.Exists(expected_package_cache_file_path) then File.Delete(expected_package_cache_file_path)