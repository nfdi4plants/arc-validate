module TestUtils

open Expecto
open ARCExpect

module Expect =
    
    let validationResultEqualIgnoringOriginal (actual: ARCExpect.ValidationResult) (expected: ARCExpect.ValidationResult) =
        Expect.equal actual.HasFailures expected.HasFailures "HasFailures was not equal."
        Expect.equal actual.Total expected.Total "Total was not equal."
        Expect.equal actual.Passed expected.Passed "Passed was not equal."
        Expect.equal actual.Failed expected.Failed "Failed was not equal."
        Expect.equal actual.Errored expected.Errored "Errored was not equal."

    let validationSummaryEqualIgnoringOriginal (actual: ARCExpect.ValidationSummary) (expected: ARCExpect.ValidationSummary) =
        validationResultEqualIgnoringOriginal actual.Critical expected.Critical
        validationResultEqualIgnoringOriginal actual.NonCritical expected.NonCritical
        Expect.equal actual.ValidationPackage expected.ValidationPackage "ValidationPackage was not equal."
        Expect.equal actual.Payload expected.Payload "Payload was not equal."
