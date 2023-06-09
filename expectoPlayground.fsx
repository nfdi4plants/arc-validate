#r "nuget: Expecto"
#r "./src/ArcValidation/bin/Debug/netstandard2.0/ArcValidation.dll"
#r "nuget: FSharpAux, 1.1.0"

open Expecto
open Expecto.Impl
open FSharpAux
open ArcValidation


[<Tests>]
let test1 =
    testList "test1" [
        testCase "expectation1" <| fun () -> Expect.isTrue true "Must be true"  // succeeds
        testCase "expectation2" <| fun () -> Expect.isTrue false "Must be true" // fails
    ]

[<Tests>]
let test2 =
    testList "test2" [
        testCase "expectation3" <| fun () -> Expect.equal 5 4 "Must be equal"           // fails
        testCase "expectation4" <| fun () -> Expect.notEqual 5 4 "Must be not equal"    // succeeds
    ]

[<Tests>]
let test3 =
    testList "test3" [
        testList "test3NestedLvl1" [
            testList "test3NestedLvl2" [
                testList "test3NestedLvl3" [
                    testList "test3NestedLvl4" [
                        testCase "expectation5" <| fun () -> Expect.contains ["hello"] "hello" "Must contain element"   // succeeds
                        testCase "expectation6" <| fun () -> Expect.contains ["hello"] "World" "Must contain element"   // fails
                    ]
                ]
            ]
        ]
    ]

[<Tests>]
let combinedTests = testList "combined" [test1; test2; test3]

let testRunSummary = performTest combinedTests

testRunSummary.failed |> List.map snd

let failTest = testCase "definite fail" <| fun () -> Expect.isTrue false "definite"

let failSumm = performTest failTest

writeJUnitSummary "./failSumm.xml" failSumm

/// Prints the actual message of an error message, without the error stack.
let truncateMsg msg = 
    try String.toLines msg
        |> Seq.head
    with _ -> msg

let allFails = failSumm.errored @ failSumm.failed

let allFailsSumm = List.map snd allFails

let allFailsSummRes = allFailsSumm.Head.result

let allFailsSummResMsg = match allFailsSummRes with | Failed s -> s | _ -> ""

truncateMsg allFailsSummResMsg

String.toLines allFailsSummResMsg