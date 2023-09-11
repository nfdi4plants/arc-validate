module ARCExpectTests

open Expecto
open ARCExpect

[<Tests>]
let ``ByValue tests`` =
    testList "ArcExpect.ByValue" [
        test "equals" {ARCExpect.ByValue.equals "Kevin" ReferenceObjects.CvParams.``Investigation Person First Name``}
        test "contains" {ARCExpect.ByValue.contains ["Kevin"; "Kevin2"] ReferenceObjects.CvParams.``Investigation Person First Name``}
    ]

[<Tests>]
let ``ByTerm tests`` =
    testList "ArcExpect.ByTerm" [
        test "equals" {
            ARCExpect.ByTerm.equals 
                ReferenceObjects.CvTerms.``Investigation Person First Name`` 
                ReferenceObjects.CvParams.``Investigation Person First Name``
        }
    ]

[<Tests>]
let ``Valid tests`` =
    testList "ArcExpect.Valid" [
        test "email" {
            ARCExpect.Valid.email ReferenceObjects.CvParams.``Investigation Person Email``
        }
    ]