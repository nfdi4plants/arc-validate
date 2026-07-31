namespace ARCExpect

open System
open Fable.Core
open Fable.Pyxpecto.Model
open Thoth.Json.Core

[<RequireQualifiedAccess>]
module private PyxpectoResultAdapter =

    let private elapsedMilliseconds (started: DateTime) =
        (DateTime.UtcNow - started).TotalMilliseconds

    let private skipped flatTest message =
        CaseResult.create(
            List.toArray flatTest.name,
            CaseOutcome.skipped(message)
        )

    let private runCase (flatTest: FlatTest) =
        async {
            let started = DateTime.UtcNow

            try
                match flatTest.test with
                | TestCode.Sync body -> body()
                | TestCode.Async body -> do! body

                return
                    CaseResult.create(
                        List.toArray flatTest.name,
                        CaseOutcome.passed(),
                        DurationMilliseconds = elapsedMilliseconds started
                    )
            with
            | :? AssertException as error ->
                return
                    CaseResult.create(
                        List.toArray flatTest.name,
                        CaseOutcome.failed(error.Message),
                        DurationMilliseconds = elapsedMilliseconds started
                    )
            | error ->
                return
                    CaseResult.create(
                        List.toArray flatTest.name,
                        CaseOutcome.errored(error.Message),
                        DurationMilliseconds = elapsedMilliseconds started
                    )
        }

    let run suiteName testCase =
        async {
            let started = DateTime.UtcNow
            let flatTests = Fable.Pyxpecto.Pyxpecto.Util.flattenTests testCase
            let hasFocused =
                flatTests
                |> List.exists (fun test -> test.focusState = FocusState.Focused)
            let results = ResizeArray<CaseResult>()

            for flatTest in flatTests do
                match hasFocused, flatTest.focusState with
                | _, FocusState.Pending ->
                    results.Add(skipped flatTest "pending")
                | true, FocusState.Normal ->
                    results.Add(
                        skipped flatTest "unfocused because focused tests are present"
                    )
                | false, FocusState.Focused
                | true, FocusState.Focused
                | false, FocusState.Normal ->
                    let! result = runCase flatTest
                    results.Add result

            return
                ValidationResult.create(
                    results.ToArray(),
                    DurationMilliseconds = elapsedMilliseconds started,
                    SuiteName = suiteName
                )
        }

[<AttachMembers>]
type Execute =

    static member Validation(
        arcValidationPackage: ARCValidationPackage,
        ?Payload: Json
    ) =
        async {
            let! critical =
                PyxpectoResultAdapter.run
                    "Critical"
                    arcValidationPackage.CriticalValidationCases
            let! nonCritical =
                PyxpectoResultAdapter.run
                    "NonCritical"
                    arcValidationPackage.NonCriticalValidationCases

            return
                ValidationSummary.create(
                    critical,
                    nonCritical,
                    ValidationPackageSummary.fromMetadata arcValidationPackage.Metadata,
                    ?Payload = Payload
                )
        }
