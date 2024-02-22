module ErrorMessageTests


open Expecto
open ARCExpect
open ControlledVocabulary


let dummyIParam = CvParam("test:0", "testTerm", "test", ParamValue.Value "no val")
let dummyIParamColl = List.init 3 (fun _ -> dummyIParam)


[<Tests>]
let ``ErrorMessage tests`` =
    testList "ErrorMessage" [
        testList "ofIParamCollection" [
            testCase "resolves correctly" <| fun _ ->
                let eMsg = ErrorMessage.ofIParamCollection "does not satisfy" dummyIParamColl
                Expect.equal eMsg "['testTerm', ..] does not satisfy\n" "resolved incorrectly"
        ]
    ]