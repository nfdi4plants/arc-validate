module ReferenceObjects

open System.IO
open JUnit
open System
open type System.Environment

let ``invenio test arc validation results`` = ValidationResults.fromJUnitFile "fixtures/xml/inveniotestarc/arc-validate-results.xml"

let application_data_path = Environment.GetFolderPath(SpecialFolder.ApplicationData, SpecialFolderOption.Create)

let expected_config_folder_path = Path.Combine(application_data_path, "nfdi4plants/arc-validate").Replace("\\", "/")
let expected_config_file_path = Path.Combine(expected_config_folder_path, "packages-config.json").Replace("\\", "/")

let expected_package_cache_folder_path = Path.Combine(expected_config_folder_path, "arc-validation-packages-cache").Replace("\\", "/")
let expected_package_cache_file_path = Path.Combine(expected_package_cache_folder_path, "packages-cache.json").Replace("\\", "/")