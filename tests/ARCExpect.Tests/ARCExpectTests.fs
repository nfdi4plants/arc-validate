module ARCExpectTests

open Expecto
open ARCExpect

[<Tests>]
let ``Validate.Param API tests`` =
    testList "Validate API tests" [
        testList "Param" [

            test "ValueIsNotEmpty passes if valid" {Validate.Param.ValueIsNotEmpty ReferenceObjects.CvParams.``Investigation Person First Name``}
            test "ValueIsNotEmpty fails if invalid" {
                Expect.throws 
                    (fun () -> Validate.Param.ValueIsNotEmpty ReferenceObjects.CvParams.``Empty Value``) 
                    "empty cvparam was not correctly detected as empty"
            }

            test "ValueIsEqualTo passes if valid" {Validate.Param.ValueIsEqualTo "Kevin" ReferenceObjects.CvParams.``Investigation Person First Name``}
            test "ValueIsEqualTo fails if invalid" {
                Expect.throws 
                    (fun () -> Validate.Param.ValueIsEqualTo "Kevin2" ReferenceObjects.CvParams.``Investigation Person First Name``) 
                    "inequal values were not correctly detected as inequal"
            }

            test "TermIsEqualTo passes if valid" {
                Validate.Param.TermIsEqualTo 
                    ReferenceObjects.CvTerms.``Investigation Person First Name`` 
                    ReferenceObjects.CvParams.``Investigation Person First Name``
            }
            test "TermIsEqualTo fails if invalid" {
                Expect.throws 
                    (fun () -> Validate.Param.TermIsEqualTo ReferenceObjects.CvTerms.``Investigation Person Email`` ReferenceObjects.CvParams.``Investigation Person First Name``) 
                    "inequal cvterms were not correctly detected as inequal"
            }

            test "ValueMatchesPattern passes if valid" {Validate.Param.ValueMatchesPattern @"^[^@\s]+@[^@\s]+\.[^@\s]+$" ReferenceObjects.CvParams.``Investigation Person Email (valid)``}
            test "ValueMatchesPattern fails if invalid" {
                Expect.throws 
                    (fun () ->Validate.Param.ValueMatchesPattern @"^[^@\s]+@[^@\s]+\.[^@\s]+$" ReferenceObjects.CvParams.``Investigation Person Email (invalid)``) 
                    "incorrect email was not correctly detected as incorrect"
            }

            test "ValueMatchesRegex passes if valid" {Validate.Param.ValueMatchesRegex StringValidationPattern.email ReferenceObjects.CvParams.``Investigation Person Email (valid)``}
            test "ValueMatchesRegex fails if invalid" {
                Expect.throws 
                    (fun () ->Validate.Param.ValueMatchesRegex StringValidationPattern.email ReferenceObjects.CvParams.``Investigation Person Email (invalid)``) 
                    "incorrect email was not correctly detected as incorrect"
            }

            test "ValueSatisfiesPredicate passes if valid" {Validate.Param.ValueSatisfiesPredicate (fun x -> (string x).Contains("Kev")) ReferenceObjects.CvParams.``Investigation Person First Name``}
            test "ValueSatisfiesPredicate fails if invalid" {
                Expect.throws 
                    (fun () -> Validate.Param.ValueSatisfiesPredicate (fun x -> (string x).Contains("XYZ")) ReferenceObjects.CvParams.``Investigation Person First Name``) 
                    "no match expected, but matched."
            }
        ]
    ]

[<Tests>]
let ``Validate.ParamCollection API tests`` =
    testList "Validate API tests" [
        testList "ParamCollection" [

            test "ContainsParamWithValue passes if valid" {Validate.ParamCollection.ContainsParamWithValue "Kevin" [ReferenceObjects.CvParams.``Investigation Person First Name``]}
            test "ContainsParamWithValue fails if invalid" {
                Expect.throws 
                    (fun () -> Validate.ParamCollection.ContainsParamWithValue "Kevin2" [ReferenceObjects.CvParams.``Investigation Person First Name``]) 
                    "non-contained value was not correctly detected as not contained"
            }

            
            test "ContainsParamWithTerm passes if valid" {Validate.ParamCollection.ContainsParamWithTerm ReferenceObjects.CvTerms.``Investigation Person First Name`` [ReferenceObjects.CvParams.``Investigation Person First Name``] }
            test "ContainsParamWithTerm fails if invalid" {
                Expect.throws 
                    (fun () -> Validate.ParamCollection.ContainsParamWithTerm ReferenceObjects.CvTerms.``Investigation Person First Name`` [ReferenceObjects.CvParams.``Investigation Person Email (valid)``] ) 
                    "non-contained cvparam was not correctly detected as not contained"
            }
        ]
    ]

[<Tests>]
let ``Validate composite/top level API tests`` =
    testList "Validate API tests" [
        testList "composite validation cases" [
            test "email passes if valid" {Validate.email ReferenceObjects.CvParams.``Investigation Person Email (valid)``}
            test "email fails if invalid" {
                Expect.throws 
                    (fun () -> Validate.email ReferenceObjects.CvParams.``Investigation Person Email (invalid)``) 
                    "invalid email was not correctly detected as invalid"}
        ]
    ]