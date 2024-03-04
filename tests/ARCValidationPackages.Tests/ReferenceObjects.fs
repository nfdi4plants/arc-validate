module ReferenceObjects

open System
open System.IO
open type System.Environment
open ARCValidationPackages
open Common.TestUtils
open TestUtils
open AVPRIndex.Domain


let testDate1 = System.DateTimeOffset.ParseExact("2023-08-15 10:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)
let testDate2 = System.DateTimeOffset.ParseExact("2023-08-15 11:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)
let testDate3 = System.DateTimeOffset.ParseExact("2024-02-22 09:00:17 +01:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)

let testPackageIndex = 
    [|
        ValidationPackageIndex.create(
            repoPath = "arc-validate-packages/test.fsx", 
            fileName = "test@1.0.0.fsx",
            lastUpdated = testDate1,
            contentHash = "",
            metadata = ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", 1, 0, 0)
        )
    |]

let testScriptContent = """(*
---
Name: test
Description: this package is here for testing purposes only.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: true
---
*)

// this file is intended for testing purposes only.
printfn "If you can read this in your console, you successfully executed test package v1.0.0!" 

#r "nuget: ARCExpect, 1.0.1"

open ARCExpect
open Expecto

let validationCases = testList "test" [
    test "yes" {Expect.equal 1 1 "yes"}
]

validationCases
|> Execute.ValidationPipeline(
    basePath = System.Environment.CurrentDirectory,
    packageName = "test"
)"""                        .ReplaceLineEndings()

let testValidationPackage1 =
    CachedValidationPackage.create(
        "test@1.0.0.fsx",
        testDate1,
        (Path.Combine(expected_package_cache_folder_path, "test@1.0.0.fsx").Replace("\\","/")),
        ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", "this package is here for testing purposes only.", 1, 0, 0)
    )

let testValidationPackage2 =
    CachedValidationPackage.create(
        "test@1.0.0.fsx",
        testDate2,
        (Path.Combine(expected_package_cache_folder_path, "test@1.0.0.fsx").Replace("\\","/")),
        ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", "this package is here for testing purposes only.", 1, 0, 0)
    )

let testValidationPackage3FullMetadata =
    CachedValidationPackage.create(
        "test@3.0.0.fsx",
        testDate3,
        (Path.Combine(expected_package_cache_folder_path, "test@3.0.0.fsx").Replace("\\","/")),
        ValidationPackageMetadata.create(
            "test",
            "this package is here for testing purposes only.",
            "this package is here for testing purposes only.", 
            1, 
            0, 
            0,
            Publish = true,
            Authors = [|
                let author1 = new Author()
                let author2 = new Author()
                (
                    author1.FullName <- "John Doe"
                    author1.Email <- "j@d.com"
                    author1.Affiliation <- "University of Nowhere"
                    author1.AffiliationLink <- "https://nowhere.edu"
                )
                (
                    author2.FullName <- "Jane Doe"
                    author2.Email <- "jj@d.com"
                    author2.Affiliation <- "University of Somewhere"
                    author2.AffiliationLink <- "https://somewhere.edu"
                )
                author1; author2
                //Author.create(
                //    fullName = "John Doe",
                //    email = "j@d.com",
                //    Affiliation = "University of Nowhere",
                //    AffiliationLink = "https://nowhere.edu"
                //)
                //Author.create(
                //    fullName = "Jane Doe",
                //    email = "jj@d.com",
                //    Affiliation = "University of Somewhere",
                //    AffiliationLink = "https://somewhere.edu"
                //)
            |],
            Tags = [|
                let annotation1 = new OntologyAnnotation ()
                let annotation2 = new OntologyAnnotation ()
                let annotation3 = new OntologyAnnotation ()
                annotation1.Name <- "validation"
                annotation2.Name <- "my-package"
                annotation3.Name <- "thing"
                annotation1
                annotation2
                annotation3
            |],
            ReleaseNotes = "add authors and tags for further testing"
        )
    )

let testPackageCache1 = PackageCache([testValidationPackage1])
let testPackageCache2 = PackageCache([testValidationPackage2])

let testScriptPath = "fixtures/testScript.fsx"
let testScriptArgsPath = "fixtures/testScriptArgs.fsx"

let testScriptPackage = CachedValidationPackage.create("testScript", testDate1, testScriptPath, ValidationPackageMetadata())
let testScriptArgsPackage = CachedValidationPackage.create("testScriptArgs", testDate1, testScriptArgsPath, ValidationPackageMetadata())