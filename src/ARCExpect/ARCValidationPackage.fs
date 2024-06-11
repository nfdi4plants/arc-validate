namespace ARCExpect

open AVPRIndex
open Expecto

type ARCValidationPackage = 
    {
        Metadata: ValidationPackageMetadata
        CriticalValidationCases: Test
        NonCriticalValidationCases: Test
    } with
        static member create (
            metadata: ValidationPackageMetadata,
            criticalValidationCases: Test,
            nonCriticalValidationCases: Test
        ) =
            {
                Metadata = metadata
                CriticalValidationCases = criticalValidationCases
                NonCriticalValidationCases = nonCriticalValidationCases
            }

        static member create (
            metadata: ValidationPackageMetadata,
            criticalValidationCasesList: Test list,
            nonCriticalValidationCasesList: Test list,
            ?CQCHookEndpoint: string
        ) =
            let criticalCases = testList "Critical" criticalValidationCasesList
            let nonCriticalCases = testList "NonCritical" nonCriticalValidationCasesList

            ARCValidationPackage.create(
                metadata = metadata, 
                criticalValidationCases = criticalCases, 
                nonCriticalValidationCases = nonCriticalCases
            )

        static member create (
            metadata: ValidationPackageMetadata,
            ?CriticalValidationCasesList: Test list,
            ?NonCriticalValidationCasesList: Test list
        ) =
            ARCValidationPackage.create(
                metadata = metadata, 
                criticalValidationCasesList = defaultArg CriticalValidationCasesList [], 
                nonCriticalValidationCasesList = defaultArg NonCriticalValidationCasesList []
            )