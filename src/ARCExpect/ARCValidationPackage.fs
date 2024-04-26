namespace ARCExpect

open AVPRIndex
open Expecto

type ARCValidationPackage = 
    {
        Metadata: ValidationPackageMetadata
        CriticalValidationCases: Test
        NonCriticalValidationCases: Test
        CQCHookEndpoint: string option
    } with
        static member create (
            metadata: ValidationPackageMetadata,
            criticalValidationCases: Test,
            nonCriticalValidationCases: Test,
            ?CQCHookEndpoint: string
        ) =
            {
                Metadata = metadata
                CriticalValidationCases = criticalValidationCases
                NonCriticalValidationCases = nonCriticalValidationCases
                CQCHookEndpoint = CQCHookEndpoint
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
                nonCriticalValidationCases = nonCriticalCases,
                ?CQCHookEndpoint = CQCHookEndpoint
            )

        static member create (
            metadata: ValidationPackageMetadata,
            ?CriticalValidationCasesList: Test list,
            ?NonCriticalValidationCasesList: Test list,
            ?CQCHookEndpoint: string
        ) =
            ARCValidationPackage.create(
                metadata = metadata, 
                criticalValidationCasesList = defaultArg CriticalValidationCasesList [], 
                nonCriticalValidationCasesList = defaultArg NonCriticalValidationCasesList [],
                ?CQCHookEndpoint = CQCHookEndpoint
            )