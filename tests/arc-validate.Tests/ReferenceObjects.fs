﻿module ReferenceObjects

open System.IO
open JUnit
open System
open type System.Environment

let ``invenio test arc validation results`` = ValidationResults.fromJUnitFile "fixtures/xml/inveniotestarc/arc-validate-results.xml"

let test_package_script_content_v1_0_0 = """(*
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
)"""                                            .ReplaceLineEndings("\n")

let test_package_script_content_v2_0_0 = """(*
---
Name: test
MajorVersion: 2
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
printfn "If you can read this in your console, you successfully executed test package v2.0.0!" 

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
)"""                                                .ReplaceLineEndings("\n")

let test_package_script_content_v3_0_0 = """(*
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
)"""                                                                    .ReplaceLineEndings("\n")

let test_package_script_content_v5_0_0 = "let [<Literal>]PACKAGE_METADATA = \"\"\"(*
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

let ``test_package_script_content_v5_0_0-use+suffixes`` = "(*
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
