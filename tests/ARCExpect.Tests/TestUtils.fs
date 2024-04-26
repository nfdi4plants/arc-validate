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
        Expect.equal actual expected "validation summaries were not equal ignoring the original run summaries."