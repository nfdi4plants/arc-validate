namespace ARCExpect

open Expecto
open ValidationPackage.Model


type ExpectoValidationPackage =
    {
        Metadata: ValidationPackageMetadata
        CriticalValidationCases: Test
        NonCriticalValidationCases: Test
    }

    static member create(
        metadata: ValidationPackageMetadata,
        criticalValidationCases: Test,
        nonCriticalValidationCases: Test
    ) =
        {
            Metadata = metadata
            CriticalValidationCases = criticalValidationCases
            NonCriticalValidationCases = nonCriticalValidationCases
        }

    static member create(
        metadata: ValidationPackageMetadata,
        criticalValidationCasesList: Test list,
        nonCriticalValidationCasesList: Test list,
        ?CQCHookEndpoint: string
    ) =
        ExpectoValidationPackage.create(
            metadata,
            testList "Critical" criticalValidationCasesList,
            testList "NonCritical" nonCriticalValidationCasesList
        )

    static member create(
        metadata: ValidationPackageMetadata,
        ?CriticalValidationCasesList: Test list,
        ?NonCriticalValidationCasesList: Test list
    ) =
        ExpectoValidationPackage.create(
            metadata = metadata,
            criticalValidationCasesList = defaultArg CriticalValidationCasesList [],
            nonCriticalValidationCasesList = defaultArg NonCriticalValidationCasesList []
        )

[<AutoOpen>]
module ARCValidationPackageCompatibility =

    type ARCValidationPackage with

        static member create(
            metadata: ValidationPackageMetadata,
            criticalValidationCases: Test,
            nonCriticalValidationCases: Test
        ) =
            ExpectoValidationPackage.create(
                metadata,
                criticalValidationCases,
                nonCriticalValidationCases
            )

        static member create(
            metadata: ValidationPackageMetadata,
            criticalValidationCasesList: Test list,
            nonCriticalValidationCasesList: Test list,
            ?CQCHookEndpoint: string
        ) =
            ExpectoValidationPackage.create(
                metadata,
                criticalValidationCasesList,
                nonCriticalValidationCasesList,
                ?CQCHookEndpoint = CQCHookEndpoint
            )

        static member create(
            metadata: ValidationPackageMetadata,
            ?CriticalValidationCasesList: Test list,
            ?NonCriticalValidationCasesList: Test list
        ) =
            ExpectoValidationPackage.create(
                metadata = metadata,
                criticalValidationCasesList = defaultArg CriticalValidationCasesList [],
                nonCriticalValidationCasesList = defaultArg NonCriticalValidationCasesList []
            )