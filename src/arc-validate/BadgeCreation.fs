namespace ARCValidate

open ARCExpect
open AnyBadge.NET

open Expecto

module BadgeCreation =

    let createSuccessBadge (text: string) (testResults: Impl.TestRunSummary) =
        
        let max = testResults.passed.Length + testResults.failed.Length

        Badge(
            label = text,
            value = testResults.passed.Length,
            ValueSuffix = $"/{max}",
            Thresholds = 
                Map([
                    0, "#CD7F32"
                    max/2, "#C0C0C0"
                    max, "#FFD700"
                ])
        )

    let createCriticalFailBadge (text: string) (testResults: Impl.TestRunSummary) =
        
        let max = testResults.passed.Length + testResults.failed.Length

        Badge(
            label = text,
            value = testResults.passed.Length,
            ValueSuffix = $"/{max} critical tests passed",
            DefaultColor = "red"
        )