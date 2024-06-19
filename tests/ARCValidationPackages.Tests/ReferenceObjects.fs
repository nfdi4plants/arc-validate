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
            metadata = ValidationPackageMetadata.create("test","this package is here for testing purposes only.", "this package is here for testing purposes only.", 1, 0, 0)
        )
    |]

let testScriptContent = """(*
---
Name: test
Summary: this package is here for testing purposes only.
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
)"""                        .ReplaceLineEndings("\n")

let testScriptContentAVPR_test_3_0_0 = """(*
---
Name: test
MajorVersion: 3
MinorVersion: 0
PatchVersion: 0
Publish: true
Summary: this package is here for testing purposes only.
Description: this package is here for testing purposes only.
Authors:
  - FullName: John Doe
    Email: j@d.com
    Affiliation: University of Nowhere
    AffiliationLink: https://nowhere.edu
  - FullName: Jane Doe
    Email: jj@d.com
    Affiliation: University of Somewhere
    AffiliationLink: https://somewhere.edu
Tags:
  - Name: validation
  - Name: my-package
  - Name: thing
ReleaseNotes: "add authors and tags for further testing"
---
*)
 
// this file is intended for testing purposes only.
printfn "If you can read this in your console, you successfully executed test package v3.0.0!" 

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
)"""                                    .ReplaceLineEndings("\n")

let testScriptContentAVPR_test_5_0_0 = "let [<Literal>]PACKAGE_METADATA = \"\"\"(*
---
Name: test
MajorVersion: 5
MinorVersion: 0
PatchVersion: 0
Publish: true
Summary: this package is here for testing purposes only.
Description: this package is here for testing purposes only.
Authors:
  - FullName: John Doe
    Email: j@d.com
    Affiliation: University of Nowhere
    AffiliationLink: https://nowhere.edu
  - FullName: Jane Doe
    Email: jj@d.com
    Affiliation: University of Somewhere
    AffiliationLink: https://somewhere.edu
Tags:
  - Name: validation
  - Name: my-package
  - Name: thing
ReleaseNotes: Use ARCExpect v3
CQCHookEndpoint: https://avpr.nfdi4plants.org
---
*)\"\"\"

printfn \"If you can read this in your console, you successfully executed test package v5.0.0!\" 

#r \"nuget: ARCExpect, 3.0.0\"

open ARCExpect
open Expecto
let test_package =
    Setup.ValidationPackage(
        metadata = Setup.Metadata(PACKAGE_METADATA),
        CriticalValidationCases = [
            test \"yes\" {Expect.equal 1 1 \"yes\"}
        ]
    )

test_package
|> Execute.ValidationPipeline(
    basePath = System.Environment.CurrentDirectory
)"                                          .ReplaceLineEndings("\n")

let testValidationPackage1 =
    CachedValidationPackage.create(
        "test@1.0.0.fsx",
        testDate1,
        (Path.Combine(expected_package_cache_folder_path_preview, "test@1.0.0.fsx").Replace("\\","/")),
        ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", "this package is here for testing purposes only.", 1, 0, 0)
    )

let testValidationPackage2 =
    CachedValidationPackage.create(
        "test@1.0.0.fsx",
        testDate2,
        (Path.Combine(expected_package_cache_folder_path_preview, "test@1.0.0.fsx").Replace("\\","/")),
        ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", "this package is here for testing purposes only.", 1, 0, 0)
    )

let testValidationPackage3FullMetadata =
    CachedValidationPackage.create(
        "test@3.0.0.fsx",
        testDate3,
        (Path.Combine(expected_package_cache_folder_path_preview, "test@3.0.0.fsx").Replace("\\","/")),
        ValidationPackageMetadata.create(
            "test",
            "this package is here for testing purposes only.",
            "this package is here for testing purposes only.", 
            1, 
            0, 
            0,
            Publish = true,
            Authors = [|
                Author.create(
                    fullName = "John Doe",
                    email = "j@d.com",
                    Affiliation = "University of Nowhere",
                    AffiliationLink = "https://nowhere.edu"
                )
                Author.create(
                    fullName = "Jane Doe",
                    email = "jj@d.com",
                    Affiliation = "University of Somewhere",
                    AffiliationLink = "https://somewhere.edu"
                )
            |],
            Tags = [|
                OntologyAnnotation.create("validation")
                OntologyAnnotation.create("my-package")
                OntologyAnnotation.create("thing")
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

module AVPRClientDomain =
    module ValidationPackage =
        let testPackage_3_0_0 = AVPRClient.ValidationPackage(
            Name = "test",
            MajorVersion = 3,
            MinorVersion = 0,
            PatchVersion = 0,
            Summary = "this package is here for testing purposes only.",
            Description = "this package is here for testing purposes only.",
            Authors = [|
                AVPRClient.Author(
                    FullName = "John Doe",
                    Email = "j@d.com",
                    Affiliation = "University of Nowhere",
                    AffiliationLink = "https://nowhere.edu"
                )
                AVPRClient.Author(
                    FullName = "Jane Doe",
                    Email = "jj@d.com",
                    Affiliation = "University of Somewhere",
                    AffiliationLink = "https://somewhere.edu"
                )
            |],
            Tags = [|
                AVPRClient.OntologyAnnotation(Name = "validation", TermAccessionNumber = "", TermSourceREF = "")
                AVPRClient.OntologyAnnotation(Name = "my-package", TermAccessionNumber = "", TermSourceREF = "")
                AVPRClient.OntologyAnnotation(Name = "thing", TermAccessionNumber = "", TermSourceREF = "")
            |],
            ReleaseNotes = "add authors and tags for further testing",
            PackageContent = System.Text.Encoding.UTF8.GetBytes testScriptContentAVPR_test_3_0_0,
            CQCHookEndpoint = ""
    )