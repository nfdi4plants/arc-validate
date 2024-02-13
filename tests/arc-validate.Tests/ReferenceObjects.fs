module ReferenceObjects

open System.IO
open JUnit
open System
open type System.Environment

let ``invenio test arc validation results`` = ValidationResults.fromJUnitFile "fixtures/xml/inveniotestarc/arc-validate-results.xml"

let test_package_script_content_v1 = "(*
---
Name: test
Description: this package is here for testing purposes only.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
---
*)

// this file is intended for testing purposes only.
printfn \"Hello, world!\" "             .ReplaceLineEndings()

let test_package_script_content_v2 = "(*
---
Name: test
MajorVersion: 2
MinorVersion: 0
PatchVersion: 0
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
ReleaseNotes: \"add authors and tags for further testing\"
---
*)

// this file is intended for testing purposes only.
printfn \"Hello, world!\" "            .ReplaceLineEndings()