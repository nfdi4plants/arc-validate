module StringValidationPatternTests


open Expecto
open FSharpAux
open ARCExpect.StringValidationPattern
open ARCExpect.StringValidationPattern.Orcid


let dummyOrcidInRange1 = "0000-0001-5555-0000"
let dummyOrcidOutRange1 = "0000-0003-5555-0000"
let dummyOrcidInRange2 = "0009-0000-5555-0000"
let dummyOrcidOutRange2 = "0009-0100-5555-0000"
let dummyOrcidSumWrong = "0123-4567-8910-1112"
let dummyOrcidSumRight = "0123-4567-8910-1111"
let dummyOrcidWrong = "1111-2222-3333-4444"
let dummyOrcidTotallyWrong = "abcd-efgh-ijkl-mnox"
let dummyOrcidRight = "0000-0001-5874-2232"

let dummyString1 = "asd"
let dummyString2 = "asdasdasdasdasdasdasdasdasdasd"


/// Erases hyphens from ORCIDs.
let noHyphens orcid =
    String.replace "-" "" orcid


[<Tests>]
let ``StringValidationPattern tests`` =
    testList "StringValidationPattern" [
        testList "Orcid" [
            testList "checkRange" [
                testCase "In range 1" <| fun _ ->
                    Expect.isTrue (noHyphens dummyOrcidInRange1 |> checkRange) "ORCID is not in range 1"
                testCase "Out of range 1" <| fun _ ->
                    Expect.isFalse (noHyphens dummyOrcidOutRange1 |> checkRange) "ORCID is in range 1 (though it mustn't)"
                testCase "In range 2" <| fun _ ->
                    Expect.isTrue (noHyphens dummyOrcidInRange2 |> checkRange) "ORCID is not in range 2"
                testCase "Out of range 2" <| fun _ ->
                    Expect.isFalse (noHyphens dummyOrcidOutRange2 |> checkRange) "ORCID is in range 2 (though it mustn't)"
            ]
            testList "checksum" [
                testCase "Correct checksum digit" <| fun _ ->
                    let lastDigit = dummyOrcidSumRight[dummyOrcidSumRight.Length - 1]
                    Expect.isTrue ((noHyphens >> checksum) dummyOrcidSumRight = lastDigit) "ORCID checksum is incorrect"
                testCase "Incorrect checksum digit" <| fun _ ->
                    let lastDigit = dummyOrcidSumWrong[dummyOrcidSumWrong.Length - 1]
                    Expect.isFalse ((noHyphens >> checksum) dummyOrcidSumWrong = lastDigit) "ORCID checksum is correct (though it mustn't)"
            ]
            testList "checkValid" [
                testCase "Is valid" <| fun _ ->
                    Expect.isTrue (checkValid dummyOrcidRight) "ORCID is invalid"
                testCase "Is invalid" <| fun _ ->
                    Expect.isFalse (checkValid dummyOrcidWrong) "ORCID is valid (though it mustn't)"
                testCase "Is totally invalid" <| fun _ ->
                    Expect.isFalse (checkValid dummyOrcidTotallyWrong) "ORCID is valid (though it mustn't)"
            ]
        ]
        testList "characterLimit" [
            testCase "is in range" <| fun _ ->
                let rx1 = characterLimit (Some 1) (Some 10)
                let rx2 = characterLimit (Some 1) None
                let rx3 = characterLimit None (Some 10)
                let rx4 = characterLimit None None
                let rx5 = characterLimit (Some 10) (Some 100)
                let rx6 = characterLimit (Some 10) None
                let rx7 = characterLimit None (Some 100)
                Expect.isTrue (rx1.Match dummyString1).Success "dummyString1 is in range of 1 to 10 chars"
                Expect.isTrue (rx2.Match dummyString1).Success "dummyString1 is in range of at least 1 chars"
                Expect.isTrue (rx3.Match dummyString1).Success "dummyString1 is in range of at most 10 chars"
                Expect.isTrue (rx4.Match dummyString1).Success "dummyString1 is in range of no char limits"
                Expect.isTrue (rx5.Match dummyString2).Success "dummyString2 is in range of 10 to 100 chars"
                Expect.isTrue (rx6.Match dummyString2).Success "dummyString2 is in range of at least 10 chars"
                Expect.isTrue (rx7.Match dummyString2).Success "dummyString2 is in range of at most 100 chars"
                Expect.isTrue (rx4.Match dummyString2).Success "dummyString2 is in range of no char limits"
            testCase "is outside of range" <| fun _ ->
                let rx1 = characterLimit (Some 1) (Some 10)
                let rx2 = characterLimit None (Some 10)
                let rx3 = characterLimit (Some 10) (Some 100)
                let rx4 = characterLimit (Some 10) None
                Expect.isFalse (rx1.Match dummyString2).Success "dummyString2 is outside of range of 1 to 10 chars"
                Expect.isFalse (rx2.Match dummyString2).Success "dummyString2 is outside of range at most 10 chars"
                Expect.isFalse (rx3.Match dummyString1).Success "dummyString1 is outside of range at 10 to 100 chars"
                Expect.isFalse (rx4.Match dummyString1).Success "dummyString1 is outside of range at least 10 chars"
        ]
    ]