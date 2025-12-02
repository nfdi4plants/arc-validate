module DomainTests

open Expecto
open ARCValidationPackages
open System
open System.IO

open ReferenceObjects
open Common.TestUtils
open TestUtils

[<Tests>]
let ``Domain tests`` =
    testList "DomainTests" [
        testList "CachedValidationPackage" [
            test "create 3_0_0" {
                let actual = CachedValidationPackage.create(
                    fileName = "test@3.0.0.fsx",
                    cacheDate = testDate3,
                    localPath = (Path.Combine(expected_package_cache_folder_path, "test@3.0.0.fsx").Replace("\\","/")),
                    metadata = AVPRIndexDomain.ValidationPackageMetadata.testPackage_3_0_0_metadata
                )
                Expect.equal actual CachedValidationPackage.testPackage_3_0_0 "CachedValidationPackage was not created correctly."
            }
            test "create 5_0_0" {
                let actual = CachedValidationPackage.create(
                    fileName = "test@5.0.0.fsx",
                    cacheDate = testDate3,
                    localPath = (Path.Combine(expected_package_cache_folder_path, "test@5.0.0.fsx").Replace("\\","/")),
                    metadata = AVPRIndexDomain.ValidationPackageMetadata.testPackage_5_0_0_metadata
                )
                Expect.equal actual CachedValidationPackage.testPackage_5_0_0 "CachedValidationPackage was not created correctly."
            }
            test "create 5_0_0-use+suffixes" {
                let actual = CachedValidationPackage.create(
                    fileName = "test@5.0.0-use+suffixes.fsx",
                    cacheDate = testDate3,
                    localPath = (Path.Combine(expected_package_cache_folder_path, "test@5.0.0-use+suffixes.fsx").Replace("\\","/")),
                    metadata = AVPRIndexDomain.ValidationPackageMetadata.``testPackage_5_0_0-use+suffixes_metadata``
                )
                Expect.equal actual CachedValidationPackage.``testPackage_5_0_0-use+suffixes`` "CachedValidationPackage was not created correctly."
            }
            test "ofPackageMetadata 5_0_0" {
                let actual = CachedValidationPackage.ofPackageMetadata(
                    packageMetadata = AVPRIndexDomain.ValidationPackageMetadata.testPackage_5_0_0_metadata,
                    Date = testDate3,
                    CacheFolder = expected_package_cache_folder_path
                )
                Expect.equal actual CachedValidationPackage.testPackage_5_0_0 "CachedValidationPackage was not created correctly."
            }
            test "ofPackageMetadata 5_0_0-use+suffixes" {
                let actual = CachedValidationPackage.ofPackageMetadata(
                    packageMetadata = AVPRIndexDomain.ValidationPackageMetadata.``testPackage_5_0_0-use+suffixes_metadata``,
                    Date = testDate3,
                    CacheFolder = expected_package_cache_folder_path
                )
                Expect.equal actual CachedValidationPackage.``testPackage_5_0_0-use+suffixes`` "CachedValidationPackage was not created correctly."
            }            
            test "updateCacheDate" {
                ()
            }            
            test "getSemanticVersionString" {
                ()
            }
        ]
    ]