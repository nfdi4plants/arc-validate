#r "nuget: Expecto"

open Expecto
open Expecto.Impl


// ##############
// INTERNAL UTILS
// ##############

let performTest test =
    let w = System.Diagnostics.Stopwatch()
    w.Start()
    evalTests Tests.defaultConfig test
    |> Async.RunSynchronously
    |> fun r -> 
        w.Stop()
        {
            results = r
            duration = w.Elapsed
            maxMemory = 0L
            memoryLimit = 0L
            timedOut = []
        }

// ##############


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