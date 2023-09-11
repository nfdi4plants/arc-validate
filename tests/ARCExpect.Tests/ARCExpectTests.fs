module ARCExpectTests

open Expecto
open ARCExpect

[<Tests>]
let ``ByValue tests`` =
    testList "ArcExpect.ByValue" [
        test "equal values are equal" {ARCExpect.ByValue.equals "Kevin" ReferenceObjects.CvParams.``Investigation Person First Name``}
        test "inequal values are inequal" {
            Expect.throws 
                (fun () -> ARCExpect.ByValue.equals "Kevin2" ReferenceObjects.CvParams.``Investigation Person First Name``) 
                "inequal values were not correctly detected as inequal"
        }

        test "contained value is detected" {ARCExpect.ByValue.contains ["Kevin"; "Kevin2"] ReferenceObjects.CvParams.``Investigation Person First Name``}
        test "non-contained value is detected" {
            Expect.throws 
                (fun () -> ARCExpect.ByValue.contains ["Kevin2"; "Kevin3"] ReferenceObjects.CvParams.``Investigation Person First Name``) 
                "non-contained value was not correctly detected as not contained"
        }

        test "correct email is matched via regex" {ARCExpect.ByValue.isMatch StringValidationPattern.email ReferenceObjects.CvParams.``Investigation Person Email (valid)``}
        test "incorrect email is not matched via regex" {
            Expect.throws 
                (fun () -> ARCExpect.ByValue.isMatch StringValidationPattern.email ReferenceObjects.CvParams.``Investigation Person Email (invalid)``) 
                "incorrect email was not correctly detected as incorrect"
        }

        test "isMatchBy detects substring" {ARCExpect.ByValue.isMatchBy (fun x -> x.Contains("Kev")) ReferenceObjects.CvParams.``Investigation Person First Name``}
        test "isMatchBy fails on no match" {
            Expect.throws 
                (fun () -> ARCExpect.ByValue.isMatchBy (fun x -> x.Contains("XYZ")) ReferenceObjects.CvParams.``Investigation Person First Name``) 
                "no match expected, but matched."
        }

        test "non-empty cvparam is not empty" {ARCExpect.ByValue.notEmpty ReferenceObjects.CvParams.``Investigation Person First Name``}
        test "empty cvparam is empty" {
            Expect.throws 
                (fun () -> ARCExpect.ByValue.notEmpty ReferenceObjects.CvParams.``Empty Value``) 
                "empty cvparam was not correctly detected as empty"
        }
    ]

[<Tests>]
let ``ByTerm tests`` =
    testList "ArcExpect.ByTerm" [
        test "equal cvterms are equal" {
            ARCExpect.ByTerm.equals 
                ReferenceObjects.CvTerms.``Investigation Person First Name`` 
                ReferenceObjects.CvParams.``Investigation Person First Name``
        }

        test "inequal cvterms are inequal" {
            Expect.throws 
                (fun () -> ARCExpect.ByTerm.equals ReferenceObjects.CvTerms.``Investigation Person Email`` ReferenceObjects.CvParams.``Investigation Person First Name``) 
                "inequal cvterms were not correctly detected as inequal"
        }

        test "contained cvparam is detected" {ARCExpect.ByTerm.exists [ReferenceObjects.CvParams.``Investigation Person First Name``] ReferenceObjects.CvParams.``Investigation Person First Name``}
        test "non-contained cvparam is detected" {
            Expect.throws 
                (fun () -> ARCExpect.ByTerm.exists [ReferenceObjects.CvParams.``Investigation Person Email (valid)``] ReferenceObjects.CvParams.``Investigation Person First Name``) 
                "non-contained cvparam was not correctly detected as not contained"
        }

    ]

[<Tests>]
let ``Valid tests`` =
    testList "ArcExpect.Valid" [
        test "valid email is valid" {ARCExpect.Valid.email ReferenceObjects.CvParams.``Investigation Person Email (valid)``}
        test "invalid email is invalid" {
            Expect.throws 
                (fun () -> ARCExpect.Valid.email ReferenceObjects.CvParams.``Investigation Person Email (invalid)``) 
                "invalid email was not correctly detected as invalid"}
    ]