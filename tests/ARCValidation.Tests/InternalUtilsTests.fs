module InternalUtilsTests

open Expecto
open FSharpAux
open ArcValidation.InternalUtils.Orcid


let dummyOrcidInRange1 = "0000-0001-5555-0000"
let dummyOrcidOutRange1 = "0000-0003-5555-0000"
let dummyOrcidInRange2 = "0009-0000-5555-0000"
let dummyOrcidOutRange2 = "0009-0100-5555-0000"
let dummyOrcidSumWrong = "0123-4567-8910-1112"
let dummyOrcidSumRight = "0123-4567-8910-1111"
let dummyOrcidWrong = "1111-2222-3333-4444"
let dummyOrcidTotallyWrong = "abcd-efgh-ijkl-mnox"
let dummyOrcidRight = "0000-0001-5874-2232"

/// Erases hyphens from ORCIDs.
let noHyphens orcid =
    String.replace "-" "" orcid

[<Tests>]
let ``InternalUtils tests`` =
    testList "InternalUtils" [
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
    ]