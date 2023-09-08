module ExpectoExtensionsTests

open ArcValidation
open ArcValidation.Configs
open Expecto


let dummyTestPassed = testCase "dummyTest1" (fun _ -> Expect.isTrue true "is not true") |> performTest
let dummyTestFailed = testCase "dummyTest2" (fun _ -> Expect.isTrue false "is not true") |> performTest

[<Tests>]
let ``Expecto extensions tests`` =
    testList "ExpectoExtensions" [
        testList "combineTestResults" [
            let combinedTsrs = combineTestRunSummaries [dummyTestFailed; dummyTestPassed]
            testCase "Combines results correctly" (fun _ ->
                let expectedResults = dummyTestFailed.results @ dummyTestPassed.results
                Expect.sequenceEqual combinedTsrs.results expectedResults "results were not correctly combined"
            )
            testCase "Combines duration correctly" (fun _ ->
                let expectedDuration = dummyTestFailed.duration + dummyTestPassed.duration
                Expect.equal combinedTsrs.duration expectedDuration "durations were not correctly combined"
            )
        ]
    ]