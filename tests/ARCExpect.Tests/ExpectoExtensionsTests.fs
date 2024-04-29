module ExpectoExtensionsTests

open ARCExpect
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
        //TO-DO: implemet these!
        ptestList "writeJUnitSummary" [
            ptest "can create correct junit file" {()}
            ptest "can write correct junit file" {()}
        ]
        ptestList "writeNUnitSummary" [
            ptest "can create correct nunit file" {()}
            ptest "can write correct nunit file" {()}
        ]
        ptestList "custom TestCaseBuilder" [
            ptest "compare tests with custom CE" {()}
        ]
    ]