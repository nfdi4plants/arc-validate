module TestUtils

open Expecto
open ARCExpect
open AVPRIndex

module Expect =
    
    let validationResultEqualIgnoringOriginal (actual: ARCExpect.ValidationResult) (expected: ARCExpect.ValidationResult) =
        let actual = { actual with OriginalRunSummary = None }
        let expected = { expected with OriginalRunSummary = None }
        Expect.isTrue (actual = expected) "validation results were not equal ignoring the original run summaries."

    let validationSummaryEqualIgnoringOriginal (actual: ARCExpect.ValidationSummary) (expected: ARCExpect.ValidationSummary) =
        let actual = { 
            actual with 
                Critical.OriginalRunSummary = None
                NonCritical.OriginalRunSummary = None
            }
        let expected = { 
            expected with 
                Critical.OriginalRunSummary = None
                NonCritical.OriginalRunSummary = None
            }
        Expect.equal actual.Critical expected.Critical "Critical Field not equal. validation summaries were not equal ignoring the original run summaries."
        Expect.equal actual.NonCritical expected.NonCritical "NonCritical Field not equal. validation summaries were not equal ignoring the original run summaries."
        Expect.equal actual.ValidationPackage expected.ValidationPackage "Critical Field not equal. validation summaries were not equal ignoring the original run summaries."
        if actual.Payload.IsSome then
            if expected.Payload.IsSome then 
                Expect.sequenceEqual (actual.Payload.Value.Keys) (expected.Payload.Value.Keys) "Payload not equal (Keys)"
                Expect.sequenceEqual (actual.Payload.Value.Keys) (expected.Payload.Value.Keys) "Payload not equal (Values)"
            else
                failwith "actual had Some Payload, but expected did not."
        else
            if expected.Payload.IsSome then 
                failwith "expected had Some Payload, but actual did not."