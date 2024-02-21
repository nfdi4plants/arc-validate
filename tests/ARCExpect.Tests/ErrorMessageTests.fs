module ErrorMessageTests


open Expecto
open ARCExpect
open ControlledVocabulary


let dummyIParam = CvParam("test:0", "testTerm", "test", ParamValue.Value "no val")
dummyIParam.AddAttribute(CvParam())


[<Tests>]
let ``ErrorMessage tests`` =
    testList "ErrorMessage" [
        testList "ofIParamCollection" [
            testCase "resolves correctly"
        ]
    ]