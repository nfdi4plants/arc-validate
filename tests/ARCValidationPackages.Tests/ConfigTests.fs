module ConfigTests

open ARCValidationPackages
open Expecto
open System.IO

open ReferenceObjects
open Common.TestUtils
open TestUtils


[<Tests>]
let ``Config tests`` = 
    testSequenced (
        testList "Config tests" [

            let testConfig =
                Config.create(
                    packageIndex = testPackageIndex,
                    indexLastUpdated = testDate1,
                    configFilePath = Defaults.CONFIG_FILE_PATH(),
                    packageCacheFolderPreview = Defaults.PACKAGE_CACHE_FOLDER_PREVIEW(),
                    packageCacheFolderRelease = Defaults.PACKAGE_CACHE_FOLDER_RELEASE()
                )

            test "config file path is correct" {
               Expect.equal 
                    (testConfig.ConfigFilePath) 
                    expected_config_file_path
                    "config file path is not correct"
            } 
            test "package cache preview folder is correct" {
                Expect.equal 
                    (testConfig.PackageCacheFolderPreview) 
                    expected_package_cache_folder_path_preview
                    "package cache preview folder path is not correct"
            } 
            test "package cache release folder is correct" {
                Expect.equal 
                    (testConfig.PackageCacheFolderRelease) 
                    expected_package_cache_folder_path_release
                    "package cache release folder path is not correct"
            } 
            test "config folder path is correct" {
                Expect.equal 
                    ((Path.GetDirectoryName (testConfig.ConfigFilePath)).Replace("\\","/"))
                    expected_config_folder_path
                    "config file path does not exist"
            } 
            test "config path exists" {
                Expect.isTrue 
                    (Path.GetDirectoryName (testConfig.ConfigFilePath) |> Directory.Exists)
                    "config file path does not exist"
            } 
            test "package cache preview folder exists" {
                Expect.isTrue 
                    (Directory.Exists(testConfig.PackageCacheFolderPreview))
                    "package cache preview folder path does not exist"
            }
            test "package cache release folder exists" {
                Expect.isTrue 
                    (Directory.Exists(testConfig.PackageCacheFolderRelease))
                    "package cache release folder path does not exist"
            } 
            test "can write json" {
                resetConfigEnvironment()
                testConfig |> Config.write()
                Expect.isTrue (File.Exists(expected_config_file_path)) "config file was not created"
            } 
            test "can read json" {
                let config = Config.read(expected_config_file_path)
                Expect.equal config testConfig "config file was not read correctly"
            } 
        ]
    )