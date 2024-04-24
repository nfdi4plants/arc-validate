module ValidationSummaryTests

open ARCExpect
open Expecto

let dummyTestPassed = testCase "dummyTest1" (fun _ -> Expect.isTrue true "is not true") |> performTest
let dummyTestFailed = testCase "dummyTest2" (fun _ -> Expect.isTrue false "is not true") |> performTest

let testPackageName = "test"
let testPackageVersion = "1.0.0"
let testHook = "http://test.com"

let testPackageNoHook = ValidationPackageSummary.create(
    name = testPackageName,
    version = testPackageVersion
)

let testPackageWithHook = ValidationPackageSummary.create(
    name = testPackageName,
    version = testPackageVersion,
    HookEndpoint = testHook
)

[<Tests>]
let ``ValidationResult tests`` =
    testList "ValidationSummary tests" [
        testList "ValidationResult" [
            test "correct result is created from passed TestRunSummary" {
                let actual = ValidationResult.ofTestRunSummary dummyTestPassed
                Expect.equal actual ReferenceObjects.ValidationResult.allPassed "validation result was not equal"
            }
            test "correct result is created from failed TestRunSummary" {
                let actual = ValidationResult.ofTestRunSummary dummyTestFailed
                Expect.equal actual ReferenceObjects.ValidationResult.allFailed "validation result was not equal"
            }
        ]
        testList "ValidationPackageSummary" [
            test "package summary is created correctly without hook" {
                let actual = ValidationPackageSummary.create(
                    name = testPackageName,
                    version = testPackageVersion
                )
                Expect.equal actual ReferenceObjects.ValidationPackageSummary.noHook "package summary was not equal"
            }
            test "package summary is created correctly with hook" {
                let actual = ValidationPackageSummary.create(
                    name = testPackageName,
                    version = testPackageVersion,
                    HookEndpoint = testHook
                )
                Expect.equal actual ReferenceObjects.ValidationPackageSummary.withHook "package summary was not equal"
            }
        ]
        testList "ValidationSummary" [
            test "critial passed, noncritical passed, package with no hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofTestRunSummaries(
                    criticalSummary = dummyTestPassed,
                    nonCriticalSummary = dummyTestPassed,
                    package = testPackageNoHook
                )
                Expect.equal actual ReferenceObjects.ValidationSummary.allPassedNoHook "validation summary was not equal"
            }
            test "critial passed, noncritical passed, package with hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofTestRunSummaries(
                    criticalSummary = dummyTestPassed,
                    nonCriticalSummary = dummyTestPassed,
                    package = testPackageWithHook
                )
                Expect.equal actual ReferenceObjects.ValidationSummary.allPassedWithHook "validation summary was not equal"
            }
            test "critial passed, noncritical failed, package with no hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofTestRunSummaries(
                    criticalSummary = dummyTestPassed,
                    nonCriticalSummary = dummyTestFailed,
                    package = testPackageNoHook
                )
                Expect.equal actual ReferenceObjects.ValidationSummary.nonCriticalFailedNoHook "validation summary was not equal"
            }
            test "critial passed, noncritical failed, package with hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofTestRunSummaries(
                    criticalSummary = dummyTestPassed,
                    nonCriticalSummary = dummyTestFailed,
                    package = testPackageWithHook
                )
                Expect.equal actual ReferenceObjects.ValidationSummary.nonCriticalFailedWithHook "validation summary was not equal"
            }

            test "critial failed, noncritical passed, package with no hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofTestRunSummaries(
                    criticalSummary = dummyTestFailed,
                    nonCriticalSummary = dummyTestPassed,
                    package = testPackageNoHook
                )
                Expect.equal actual ReferenceObjects.ValidationSummary.criticalFailedNoHook "validation summary was not equal"
            }
            test "critial failed, noncritical passed, package with hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofTestRunSummaries(
                    criticalSummary = dummyTestFailed,
                    nonCriticalSummary = dummyTestPassed,
                    package = testPackageWithHook
                )
                Expect.equal actual ReferenceObjects.ValidationSummary.criticalFailedWithHook "validation summary was not equal"
            }
            test "critial failed, noncritical failed, package with no hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofTestRunSummaries(
                    criticalSummary = dummyTestFailed,
                    nonCriticalSummary = dummyTestFailed,
                    package = testPackageNoHook
                )
                Expect.equal actual ReferenceObjects.ValidationSummary.allFailedNoHook "validation summary was not equal"
            }
            test "critial failed, noncritical failed, package with hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofTestRunSummaries(
                    criticalSummary = dummyTestFailed,
                    nonCriticalSummary = dummyTestFailed,
                    package = testPackageWithHook
                )
                Expect.equal actual ReferenceObjects.ValidationSummary.allFailedWithHook "validation summary was not equal"
            }
        ]
        testList "Serialization" [
            test "correctly serialized without hook" {
                let actual = 
                    ReferenceObjects.ValidationSummary.allPassedNoHook
                    |> ValidationSummary.toJson
                Expect.equal actual ReferenceObjects.ValidationSummary.allPassedNoHookJson "serialization was not equal"
            }
            test "roundtrip without hook" {
                let actual = 
                    ReferenceObjects.ValidationSummary.allPassedNoHook
                    |> ValidationSummary.toJson
                    |> ValidationSummary.fromJson

                Expect.equal actual ReferenceObjects.ValidationSummary.allPassedNoHook "roundtrip was not equal"
            }
            test "correctly serialized with hook" {
                let actual = 
                    ReferenceObjects.ValidationSummary.allPassedWithHook
                    |> ValidationSummary.toJson
                Expect.equal actual ReferenceObjects.ValidationSummary.allPassedWithHookJson "serialization was not equal"
            }
            test "roundtrip with hook" {
                let actual = 
                    ReferenceObjects.ValidationSummary.allPassedWithHook
                    |> ValidationSummary.toJson
                    |> ValidationSummary.fromJson

                Expect.equal actual ReferenceObjects.ValidationSummary.allPassedWithHook "roundtrip was not equal"
            }
        ]
    ]