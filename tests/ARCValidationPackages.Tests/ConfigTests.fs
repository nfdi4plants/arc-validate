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
                    packageCacheFolder = Defaults.PACKAGE_CACHE_FOLDER(),
                    configFilePath = Defaults.CONFIG_FILE_PATH(),
                    isAPI = false,
                    packageIndex = testPackageIndex,
                    indexLastUpdated = testDate1
                )

            test "config file path is correct" {
               Expect.equal 
                    (testConfig.ConfigFilePath) 
                    expected_config_file_path
                    "config file path is not correct"
            } 
            test "package cache folder is correct" {
                Expect.equal 
                    (testConfig.PackageCacheFolder) 
                    expected_package_cache_folder_path
                    "package cache folder path is not correct"
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
            test "package cache folder exists" {
                Expect.isTrue 
                    (Directory.Exists(testConfig.PackageCacheFolder))
                    "package cache folder path does not exist"
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