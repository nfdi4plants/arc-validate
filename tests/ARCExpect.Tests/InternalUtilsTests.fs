module InternalUtilsTests


open Expecto
open FSharpAux
open ControlledVocabulary
open OBO.NET

open ARCExpect
open ARCExpect.StringValidationPattern.Orcid


[<Tests>]
let ``InternalUtils tests`` =
    testList "InternalUtils" [
        testList "OboTerm" [
            testList "toCvTerm" [
                testCase "Correct CvTerm" <| fun _ ->
                    let expected = CvTerm.create("test:000", "testTerm0", "test")
                    let actual = OboTerm.Create("test:000", Name = "testTerm0") |> OboTerm.toCvTerm
                    Expect.equal actual expected "CvTerm from OboTerm does not match expected CvTerm"
            ]
        ]
    ]