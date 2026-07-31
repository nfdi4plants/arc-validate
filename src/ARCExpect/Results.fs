namespace ARCExpect

open Fable.Core

type CaseOutcomeKind =
    | Passed = 0
    | Failed = 1
    | Errored = 2
    | Skipped = 3

[<AttachMembers>]
type CaseOutcome(kind: CaseOutcomeKind, message: string, stackTrace: string) =

    let _kind = kind
    let _message = if isNull message then "" else message
    let _stackTrace = if isNull stackTrace then "" else stackTrace

    member _.Kind = _kind
    member _.Message = _message
    member _.StackTrace = _stackTrace

    static member create(kind: CaseOutcomeKind, ?Message: string, ?StackTrace: string) =
        CaseOutcome(kind, defaultArg Message "", defaultArg StackTrace "")

    static member passed() = CaseOutcome.create(CaseOutcomeKind.Passed)

    static member failed(message: string) =
        CaseOutcome.create(CaseOutcomeKind.Failed, Message = message)

    static member errored(message: string, ?StackTrace: string) =
        CaseOutcome.create(
            CaseOutcomeKind.Errored,
            Message = message,
            ?StackTrace = StackTrace
        )

    static member skipped(message: string) =
        CaseOutcome.create(CaseOutcomeKind.Skipped, Message = message)

    override this.Equals(other) =
        match other with
        | :? CaseOutcome as outcome ->
            this.Kind = outcome.Kind
            && this.Message = outcome.Message
            && this.StackTrace = outcome.StackTrace
        | _ -> false

    override this.GetHashCode() =
        hash (this.Kind, this.Message, this.StackTrace)

[<AttachMembers>]
type CaseResult(name: string array, outcome: CaseOutcome, durationMilliseconds: float) =

    let _name = if isNull name then Array.empty else name
    let _outcome = outcome
    let _durationMilliseconds = durationMilliseconds

    member _.Name = _name
    member _.Outcome = _outcome
    member _.DurationMilliseconds = _durationMilliseconds
    member _.DurationSeconds = _durationMilliseconds / 1000.0

    member _.FullName =
        if _name.Length = 0 then
            "[ ]"
        else
            "[ " + String.concat "; " _name + " ]"

    static member create(
        name: string array,
        outcome: CaseOutcome,
        ?DurationMilliseconds: float
    ) =
        CaseResult(name, outcome, defaultArg DurationMilliseconds 0.0)

    override this.Equals(other) =
        match other with
        | :? CaseResult as result ->
            this.Name = result.Name
            && this.Outcome = result.Outcome
            && this.DurationMilliseconds = result.DurationMilliseconds
        | _ -> false

    override this.GetHashCode() =
        hash (this.Name, this.Outcome, this.DurationMilliseconds)

[<AttachMembers>]
type RunSummary(cases: CaseResult array, durationMilliseconds: float, suiteName: string) =

    let _cases = if isNull cases then Array.empty else cases
    let _durationMilliseconds = durationMilliseconds
    let _suiteName = if isNull suiteName then "" else suiteName

    member _.Cases = _cases
    member _.DurationMilliseconds = _durationMilliseconds
    member _.SuiteName = _suiteName
    member _.Total = _cases.Length

    member _.Passed =
        _cases
        |> Array.filter (fun result -> result.Outcome.Kind = CaseOutcomeKind.Passed)
        |> Array.length

    member _.Failed =
        _cases
        |> Array.filter (fun result -> result.Outcome.Kind = CaseOutcomeKind.Failed)
        |> Array.length

    member _.Errored =
        _cases
        |> Array.filter (fun result -> result.Outcome.Kind = CaseOutcomeKind.Errored)
        |> Array.length

    member _.Skipped =
        _cases
        |> Array.filter (fun result -> result.Outcome.Kind = CaseOutcomeKind.Skipped)
        |> Array.length

    member this.HasFailures = this.Failed > 0 || this.Errored > 0
    member this.Successful = not this.HasFailures

    static member create(
        cases: CaseResult array,
        ?DurationMilliseconds: float,
        ?SuiteName: string
    ) =
        RunSummary(
            cases,
            defaultArg DurationMilliseconds 0.0,
            defaultArg SuiteName ""
        )

    static member empty(?SuiteName: string) =
        RunSummary.create(Array.empty, ?SuiteName = SuiteName)

    static member combine(summaries: RunSummary array) =
        if isNull summaries || summaries.Length = 0 then
            RunSummary.empty()
        else
            let suiteName =
                summaries
                |> Array.tryPick (fun summary ->
                    if summary.SuiteName = "" then None else Some summary.SuiteName
                )
                |> Option.defaultValue ""

            RunSummary.create(
                summaries |> Array.collect (fun summary -> summary.Cases),
                DurationMilliseconds =
                    (summaries |> Array.sumBy (fun summary -> summary.DurationMilliseconds)),
                SuiteName = suiteName
            )

    override this.Equals(other) =
        match other with
        | :? RunSummary as summary ->
            this.Cases = summary.Cases
            && this.DurationMilliseconds = summary.DurationMilliseconds
            && this.SuiteName = summary.SuiteName
        | _ -> false

    override this.GetHashCode() =
        hash (this.Cases, this.DurationMilliseconds, this.SuiteName)

[<AttachMembers>]
type ValidationResult(cases: CaseResult array, durationMilliseconds: float, suiteName: string) =
    inherit RunSummary(cases, durationMilliseconds, suiteName)

    static member create(
        cases: CaseResult array,
        ?DurationMilliseconds: float,
        ?SuiteName: string
    ) =
        ValidationResult(
            cases,
            defaultArg DurationMilliseconds 0.0,
            defaultArg SuiteName ""
        )

    static member fromCounts(
        total: int,
        passed: int,
        failed: int,
        errored: int,
        ?Skipped: int,
        ?SuiteName: string
    ) =
        let skipped = defaultArg Skipped (max 0 (total - passed - failed - errored))

        let cases =
            [|
                for index in 1 .. passed do
                    CaseResult.create(
                        [| $"passed-{index}" |],
                        CaseOutcome.passed()
                    )

                for index in 1 .. failed do
                    CaseResult.create(
                        [| $"failed-{index}" |],
                        CaseOutcome.failed("")
                    )

                for index in 1 .. errored do
                    CaseResult.create(
                        [| $"errored-{index}" |],
                        CaseOutcome.errored("")
                    )

                for index in 1 .. skipped do
                    CaseResult.create(
                        [| $"skipped-{index}" |],
                        CaseOutcome.skipped("")
                    )
            |]

        ValidationResult.create(cases, ?SuiteName = SuiteName)

    static member fromLegacyCounts(
        hasFailures: bool,
        total: int,
        passed: int,
        failed: int,
        errored: int,
        ?SuiteName: string
    ) =
        let adjustedFailed =
            if hasFailures && failed = 0 && errored = 0 then 1 else failed

        ValidationResult.fromCounts(
            max total (passed + adjustedFailed + errored),
            passed,
            adjustedFailed,
            errored,
            ?SuiteName = SuiteName
        )
