module ValidationSummaryTests

open ARCExpect
open Expecto
open TestUtils
open Thoth.Json.Core

    
let dummyTestPassed = ReferenceObjects.TestCase.dummyTestWillPass |> performTest
let dummyTestFailed = ReferenceObjects.TestCase.dummyTestWillFail |> performTest

let testPackageName = "test"
let testPackageVersion = "1.0.0"

let testSummaryNoHook = "A package without CQC hook."
let testDescriptionNoHook = "A package without CQC hook. More text here."

let testSummaryWithHook = "A package with CQC hook."
let testDescriptionWithHook = "A package with CQC hook. More text here."
let testHook = "http://test.com"

let testPackageNoHook = ValidationPackageSummary.create(
    name = testPackageName,
    version = testPackageVersion,
    summary = testSummaryNoHook,
    description = testDescriptionNoHook
)

let testPackageWithHook = ValidationPackageSummary.create(
    name = testPackageName,
    version = testPackageVersion,
    summary = testSummaryWithHook,
    description = testDescriptionWithHook,
    CQCHookEndpoint = testHook
)

let testPayload = 
    Json.Object [ "key1", Json.String "value1"; "key2", Json.Number 2.0 ]

[<Tests>]
let ``ValidationResult tests`` =
    testList "ValidationSummary tests" [
        testList "ValidationResult" [
            test "correct result is created from passed TestRunSummary" {
                let actual = ValidationResult.ofExpectoTestRunSummary dummyTestPassed
                Expect.validationResultEqualIgnoringOriginal actual ReferenceObjects.ValidationResult.allPassed 
            }
            test "correct result is created from failed TestRunSummary" {
                let actual = ValidationResult.ofExpectoTestRunSummary dummyTestFailed
                Expect.validationResultEqualIgnoringOriginal actual ReferenceObjects.ValidationResult.allFailed 
            }
        ]
        testList "ValidationPackageSummary" [
            test "package summary is created correctly without hook" {
                let actual = ValidationPackageSummary.create(
                    name = testPackageName,
                    version = testPackageVersion,
                    summary = testSummaryNoHook,
                    description = testDescriptionNoHook
                )
                Expect.equal actual ReferenceObjects.ValidationPackageSummary.noHook "package summary was not equal"
            }
            test "package summary is created correctly with hook" {
                let actual = ValidationPackageSummary.create(
                    name = testPackageName,
                    version = testPackageVersion,
                    summary = testSummaryWithHook,
                    description = testDescriptionWithHook,
                    CQCHookEndpoint = testHook
                )
                Expect.equal actual ReferenceObjects.ValidationPackageSummary.withHook "package summary was not equal"
            }
        ]
        testList "ValidationSummary" [
            test "critial passed, noncritical passed, package with no hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofExpectoTestRunSummaries(
                    criticalSummary = dummyTestPassed,
                    nonCriticalSummary = dummyTestPassed,
                    package = testPackageNoHook
                )
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.allPassedNoHook 
            }
            test "critial passed, noncritical passed, package with hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofExpectoTestRunSummaries(
                    criticalSummary = dummyTestPassed,
                    nonCriticalSummary = dummyTestPassed,
                    package = testPackageWithHook
                )
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.allPassedWithHook
            }
            test "critial passed, noncritical passed, package with payload is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofExpectoTestRunSummaries(
                    criticalSummary = dummyTestPassed,
                    nonCriticalSummary = dummyTestPassed,
                    package = testPackageWithHook,
                    Payload = testPayload
                )
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.allPassedWithPayload
            }

            test "critial passed, noncritical failed, package with no hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofExpectoTestRunSummaries(
                    criticalSummary = dummyTestPassed,
                    nonCriticalSummary = dummyTestFailed,
                    package = testPackageNoHook
                )
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.nonCriticalFailedNoHook
            }
            test "critial passed, noncritical failed, package with hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofExpectoTestRunSummaries(
                    criticalSummary = dummyTestPassed,
                    nonCriticalSummary = dummyTestFailed,
                    package = testPackageWithHook
                )
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.nonCriticalFailedWithHook
            }

            test "critial failed, noncritical passed, package with no hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofExpectoTestRunSummaries(
                    criticalSummary = dummyTestFailed,
                    nonCriticalSummary = dummyTestPassed,
                    package = testPackageNoHook
                )
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.criticalFailedNoHook
            }
            test "critial failed, noncritical passed, package with hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofExpectoTestRunSummaries(
                    criticalSummary = dummyTestFailed,
                    nonCriticalSummary = dummyTestPassed,
                    package = testPackageWithHook
                )
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.criticalFailedWithHook
            }
            test "critial failed, noncritical failed, package with no hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofExpectoTestRunSummaries(
                    criticalSummary = dummyTestFailed,
                    nonCriticalSummary = dummyTestFailed,
                    package = testPackageNoHook
                )
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.allFailedNoHook
            }
            test "critial failed, noncritical failed, package with hook is created correctly from TestRunSummaries" {
                let actual = ValidationSummary.ofExpectoTestRunSummaries(
                    criticalSummary = dummyTestFailed,
                    nonCriticalSummary = dummyTestFailed,
                    package = testPackageWithHook
                )
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.allFailedWithHook
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
            test "correctly serialized with payload" {
                let actual = 
                    ReferenceObjects.ValidationSummary.allPassedWithPayload
                    |> ValidationSummary.toJson
                Expect.equal actual ReferenceObjects.ValidationSummary.allPassedWithPayloadJson "serialization was not equal"
            }
            test "roundtrip with payload" {
                let actual = 
                    ReferenceObjects.ValidationSummary.allPassedWithPayload
                    |> ValidationSummary.toJson
                    |> ValidationSummary.fromJson

                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.allPassedWithPayload
            }
            test "roundtrip preserves aggregate results without compatibility state" {
                
                let initial = ValidationSummary.ofExpectoTestRunSummaries(dummyTestPassed, dummyTestPassed, ReferenceObjects.ValidationPackageSummary.noHook)
                let actual = 
                    ValidationSummary.ofExpectoTestRunSummaries(dummyTestPassed, dummyTestPassed, ReferenceObjects.ValidationPackageSummary.noHook)
                    |> ValidationSummary.toJson
                    |> ValidationSummary.fromJson

                Expect.validationSummaryEqualIgnoringOriginal actual initial
                Expect.equal initial.Critical.Cases[0].Name [| "dummyTest1" |] "Expecto case name was not retained in memory"
                Expect.equal actual.Critical.Total initial.Critical.Total "Critical aggregate was not retained"
                Expect.equal actual.NonCritical.Total initial.NonCritical.Total "NonCritical aggregate was not retained"

            }
        ]
    ]
