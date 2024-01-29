module ReferenceObjects

open System.IO
open JUnit
open System
open type System.Environment

let ``invenio test arc validation results`` = ValidationResults.fromJUnitFile "fixtures/xml/inveniotestarc/arc-validate-results.xml"