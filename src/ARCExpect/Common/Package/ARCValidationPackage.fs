namespace ARCExpect

open Fable.Core
open Fable.Pyxpecto.Model
open ValidationPackage.Model

[<AttachMembers>]
type ARCValidationPackage private (
    metadata: ValidationPackageMetadata,
    criticalValidationCases: TestCase,
    nonCriticalValidationCases: TestCase
) =

    let _metadata = metadata
    let _criticalValidationCases = criticalValidationCases
    let _nonCriticalValidationCases = nonCriticalValidationCases

    member _.Metadata = _metadata
    member _.CriticalValidationCases = _criticalValidationCases
    member _.NonCriticalValidationCases = _nonCriticalValidationCases

    static member create(
        metadata: ValidationPackageMetadata,
        ?CriticalValidationCases: TestCase array,
        ?NonCriticalValidationCases: TestCase array
    ) =
        ARCValidationPackage(
            metadata,
            TestList(
                "Critical",
                defaultArg CriticalValidationCases [||] |> Array.toList,
                Normal
            ),
            TestList(
                "NonCritical",
                defaultArg NonCriticalValidationCases [||] |> Array.toList,
                Normal
            )
        )

    static member createFromTests(
        metadata: ValidationPackageMetadata,
        criticalValidationCases: TestCase,
        nonCriticalValidationCases: TestCase
    ) =
        ARCValidationPackage(
            metadata,
            criticalValidationCases,
            nonCriticalValidationCases
        )
