module ReferenceObjects

open System.IO
open JUnit
open System
open type System.Environment

let ``invenio test arc validation results`` = ValidationResults.fromJUnitFile "fixtures/xml/inveniotestarc/arc-validate-results.xml"

let test_package_script_content_v1 = """(*
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
)"""                                            .ReplaceLineEndings()

let test_package_script_content_v2 = """(*
---
Name: test
MajorVersion: 2
MinorVersion: 0
PatchVersion: 0
Publish: true
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
  - validation
  - my-package
  - thing
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
)"""                                    .ReplaceLineEndings()