module ReferenceObjects

open System
open System.IO
open type System.Environment
open ARCValidationPackages
open Common.TestUtils
open TestUtils
open ValidationPackage.Model


let testDate1 = System.DateTimeOffset.ParseExact("2023-08-15 10:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)
let testDate2 = System.DateTimeOffset.ParseExact("2023-08-15 11:00:00 +02:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)
let testDate3 = System.DateTimeOffset.ParseExact("2024-02-22 09:00:17 +01:00", "yyyy-MM-dd HH:mm:ss zzz", System.Globalization.CultureInfo.InvariantCulture)

let fsharpTestScriptPath = "fixtures/testScript.fsx"
let fsharpTestScriptArgsPath = "fixtures/testScriptArgs.fsx"

let pythonTestScriptPath = "fixtures/testScript.py"
let pythonTestScriptArgsPath = "fixtures/testScriptArgs.py"

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

let ``testScriptContentAVPR_test_5_0_0-use+suffixes`` = "(*
---
Name: test
MajorVersion: 5
MinorVersion: 0
PatchVersion: 0
PreReleaseVersionSuffix: use
BuildMetadataVersionSuffix: suffixes
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
ReleaseNotes: Use pre-release and build metadata version suffixes 
CQCHookEndpoint: https://avpr.nfdi4plants.org
---
*)

printfn \"If you can read this in your console, you successfully executed test package v5.0.0-use+suffixes!\" ".ReplaceLineEndings("\n")

module ValidationPackageModel =

    module ValidationPackageMetadata =

        let testPackage_3_0_0_metadata = ValidationPackageMetadata.create(
            name = "test",
            majorVersion = 3,
            minorVersion = 0,
            patchVersion = 0,
            summary = "this package is here for testing purposes only.",
            description = "this package is here for testing purposes only.",
            programmingLanguage = "FSharp",
            Authors = [|
                ValidationPackage.Model.Author.create(
                    fullName = "John Doe",
                    Email = "j@d.com",
                    Affiliation = "University of Nowhere",
                    AffiliationLink = "https://nowhere.edu"
                )
                ValidationPackage.Model.Author.create(
                    fullName = "Jane Doe",
                    Email = "jj@d.com",
                    Affiliation = "University of Somewhere",
                    AffiliationLink = "https://somewhere.edu"
                )
            |],
            Tags = [|
                ValidationPackage.Model.OntologyAnnotation.create(name = "validation")
                ValidationPackage.Model.OntologyAnnotation.create(name = "my-package")
                ValidationPackage.Model.OntologyAnnotation.create(name = "thing")
            |],
            ReleaseNotes = "add authors and tags for further testing",
            CQCHookEndpoint = ""
        )

        let testPackage_5_0_0_metadata = ValidationPackageMetadata.create(
            name = "test",
            majorVersion = 5,
            minorVersion = 0,
            patchVersion = 0,
            summary = "this package is here for testing purposes only.",
            description = "this package is here for testing purposes only.",
            programmingLanguage = "FSharp",
            Authors = [|
                ValidationPackage.Model.Author.create(
                    fullName = "John Doe",
                    Email = "j@d.com",
                    Affiliation = "University of Nowhere",
                    AffiliationLink = "https://nowhere.edu"
                )
                ValidationPackage.Model.Author.create(
                    fullName = "Jane Doe",
                    Email = "jj@d.com",
                    Affiliation = "University of Somewhere",
                    AffiliationLink = "https://somewhere.edu"
                )
            |],
            Tags = [|
                ValidationPackage.Model.OntologyAnnotation.create(name = "validation")
                ValidationPackage.Model.OntologyAnnotation.create(name = "my-package")
                ValidationPackage.Model.OntologyAnnotation.create(name = "thing")
            |],
            ReleaseNotes = "Use ARCExpect v3",
            CQCHookEndpoint = "https://avpr.nfdi4plants.org"
        )
        
        let ``testPackage_5_0_0-use+suffixes_metadata`` = ValidationPackageMetadata.create(
            name = "test",
            majorVersion = 5,
            minorVersion = 0,
            patchVersion = 0,
            summary = "this package is here for testing purposes only.",
            description = "this package is here for testing purposes only.",
            programmingLanguage = "FSharp",
            PreReleaseVersionSuffix = "use",
            BuildMetadataVersionSuffix = "suffixes",
            Authors = [|
                ValidationPackage.Model.Author.create(
                    fullName = "John Doe",
                    Email = "j@d.com",
                    Affiliation = "University of Nowhere",
                    AffiliationLink = "https://nowhere.edu"
                )
                ValidationPackage.Model.Author.create(
                    fullName = "Jane Doe",
                    Email = "jj@d.com",
                    Affiliation = "University of Somewhere",
                    AffiliationLink = "https://somewhere.edu"
                )
            |],
            Tags = [|
                ValidationPackage.Model.OntologyAnnotation.create(name = "validation")
                ValidationPackage.Model.OntologyAnnotation.create(name = "my-package")
                ValidationPackage.Model.OntologyAnnotation.create(name = "thing")
            |],
            ReleaseNotes = "Use pre-release and build metadata version suffixes",
            CQCHookEndpoint = "https://avpr.nfdi4plants.org"
        )

module AVPRClientDomain =
    module ValidationPackage =
        let testPackage_3_0_0 = AVPRClient.ValidationPackage(
            Name = "test",
            MajorVersion = 3,
            MinorVersion = 0,
            PatchVersion = 0,
            PreReleaseVersionSuffix = "",
            BuildMetadataVersionSuffix = "",
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

        let testPackage_5_0_0 = AVPRClient.ValidationPackage(
            Name = "test",
            MajorVersion = 5,
            MinorVersion = 0,
            PatchVersion = 0,
            PreReleaseVersionSuffix = "",
            BuildMetadataVersionSuffix = "",
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
            ReleaseNotes = "Use ARCExpect v3",
            PackageContent = System.Text.Encoding.UTF8.GetBytes testScriptContentAVPR_test_5_0_0,
            CQCHookEndpoint = "https://avpr.nfdi4plants.org"
        )

        let ``testPackage_5_0_0-use+suffixes`` = AVPRClient.ValidationPackage(
            Name = "test",
            MajorVersion = 5,
            MinorVersion = 0,
            PatchVersion = 0,
            PreReleaseVersionSuffix = "use",
            BuildMetadataVersionSuffix = "suffixes",
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
            ReleaseNotes = "Use pre-release and build metadata version suffixes",
            PackageContent = System.Text.Encoding.UTF8.GetBytes ``testScriptContentAVPR_test_5_0_0-use+suffixes``,
            CQCHookEndpoint = "https://avpr.nfdi4plants.org"
        )


module CachedValidationPackage =

    let testValidationPackage1 =
        CachedValidationPackage.create(
            "test@1.0.0.fsx",
            testDate1,
            (Path.Combine(expected_package_cache_folder_path, "test@1.0.0.fsx").Replace("\\","/")),
            ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", "this package is here for testing purposes only.", 1, 0, 0, "FSharp")
        )

    let testValidationPackage2 =
        CachedValidationPackage.create(
            "test@1.0.0.fsx",
            testDate2,
            (Path.Combine(expected_package_cache_folder_path, "test@1.0.0.fsx").Replace("\\","/")),
            ValidationPackageMetadata.create("test", "this package is here for testing purposes only.", "this package is here for testing purposes only.", 1, 0, 0, "FSharp")
        )

    let testPackage_3_0_0 =
        CachedValidationPackage.create(
            fileName = "test@3.0.0.fsx",
            cacheDate = testDate3,
            localPath = (Path.Combine(expected_package_cache_folder_path, "test@3.0.0.fsx").Replace("\\","/")),
            metadata = ValidationPackageModel.ValidationPackageMetadata.testPackage_3_0_0_metadata
                
        )

    let testPackage_5_0_0 =
        CachedValidationPackage.create(
            fileName = "test@5.0.0.fsx",
            cacheDate = testDate3,
            localPath = (Path.Combine(expected_package_cache_folder_path, "test@5.0.0.fsx").Replace("\\","/")),
            metadata = ValidationPackageModel.ValidationPackageMetadata.testPackage_5_0_0_metadata
        )

    let ``testPackage_5_0_0-use+suffixes`` =
        CachedValidationPackage.create(
            fileName = "test@5.0.0-use+suffixes.fsx",
            cacheDate = testDate3,
            localPath = (Path.Combine(expected_package_cache_folder_path, "test@5.0.0-use+suffixes.fsx").Replace("\\","/")),
            metadata = ValidationPackageModel.ValidationPackageMetadata.``testPackage_5_0_0-use+suffixes_metadata``
        )

    let fsharpTestScriptPackage = CachedValidationPackage.create("fsharpTestScript", testDate1, fsharpTestScriptPath, ValidationPackageMetadata())
    let fsharpTestScriptArgsPackage = CachedValidationPackage.create("fsharpTestScriptArgs", testDate1, fsharpTestScriptArgsPath, ValidationPackageMetadata())

    let pythonTestScriptPackage = CachedValidationPackage.create("pythonTestScript", testDate1, pythonTestScriptPath, ValidationPackageMetadata())
    let pythonTestScriptArgsPackage = CachedValidationPackage.create("pythonTestScriptArgs", testDate1, pythonTestScriptArgsPath, ValidationPackageMetadata())

module PackageCache =

    let testPackageCache1 = PackageCache([CachedValidationPackage.testValidationPackage1])
    let testPackageCache2 = PackageCache([CachedValidationPackage.testValidationPackage2])
