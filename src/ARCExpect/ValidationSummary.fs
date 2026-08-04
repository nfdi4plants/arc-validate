namespace ARCExpect

open Fable.Core
open Thoth.Json.Core
open ValidationPackage.Codecs
open ValidationPackage.Model

[<AttachMembers>]
type ValidationPackageSummary(
    name: string,
    version: string,
    summary: string,
    description: string,
    cqcHookEndpoint: string
) =

    let _name = if isNull name then "" else name
    let _version = if isNull version then "" else version
    let _summary = if isNull summary then "" else summary
    let _description = if isNull description then "" else description
    let _cqcHookEndpoint = if isNull cqcHookEndpoint then "" else cqcHookEndpoint

    member _.Name = _name
    member _.Version = _version
    member _.Summary = _summary
    member _.Description = _description
    member _.CQCHookEndpoint =
        if _cqcHookEndpoint = "" then None else Some _cqcHookEndpoint

    static member create(
        name: string,
        version: string,
        summary: string,
        description: string,
        ?CQCHookEndpoint: string
    ) =
        ValidationPackageSummary(
            name,
            version,
            summary,
            description,
            defaultArg CQCHookEndpoint ""
        )

    static member fromMetadata(metadata: ValidationPackageMetadata) =
        ValidationPackageSummary.create(
            metadata.Name,
            ValidationPackageMetadata.getSemanticVersionString metadata,
            metadata.Summary,
            metadata.Description,
            ?CQCHookEndpoint =
                (if metadata.CQCHookEndpoint = "" then
                     None
                 else
                     Some metadata.CQCHookEndpoint)
        )

    override this.Equals(other) =
        match other with
        | :? ValidationPackageSummary as package ->
            this.Name = package.Name
            && this.Version = package.Version
            && this.Summary = package.Summary
            && this.Description = package.Description
            && this.CQCHookEndpoint = package.CQCHookEndpoint
        | _ -> false

    override this.GetHashCode() =
        hash (
            this.Name,
            this.Version,
            this.Summary,
            this.Description,
            this.CQCHookEndpoint
        )

module private ValidationSummaryCodec =

    let encodeResult (result: ValidationResult) =
        Encode.object [
            "HasFailures", Encode.bool result.HasFailures
            "Total", Encode.int result.Total
            "Passed", Encode.int result.Passed
            "Failed", Encode.int result.Failed
            "Errored", Encode.int result.Errored
        ]

    let resultDecoder: Decoder<ValidationResult> =
        Decode.object (fun get ->
            ValidationResult.fromCounts(
                get.Required.Field "Total" Decode.int,
                get.Required.Field "Passed" Decode.int,
                get.Required.Field "Failed" Decode.int,
                get.Required.Field "Errored" Decode.int
            )
        )

    let encodePackage (package: ValidationPackageSummary) =
        Encode.object [
            "Name", Encode.string package.Name
            "Version", Encode.string package.Version
            "Summary", Encode.string package.Summary
            "Description", Encode.string package.Description

            match package.CQCHookEndpoint with
            | Some endpoint -> "CQCHookEndpoint", Encode.string endpoint
            | None -> ()
        ]

    let packageDecoder: Decoder<ValidationPackageSummary> =
        Decode.object (fun get ->
            ValidationPackageSummary.create(
                get.Required.Field "Name" Decode.string,
                get.Required.Field "Version" Decode.string,
                get.Required.Field "Summary" Decode.string,
                get.Required.Field "Description" Decode.string,
                ?CQCHookEndpoint = get.Optional.Field "CQCHookEndpoint" Decode.string
            )
        )

[<AttachMembers>]
type ValidationSummary(
    critical: ValidationResult,
    nonCritical: ValidationResult,
    validationPackage: ValidationPackageSummary,
    payload: Json option,
    ?sourceBranch: string,
    ?sourceCommitHash: string
) =

    let _critical = critical
    let _nonCritical = nonCritical
    let _validationPackage = validationPackage
    let _payload = payload
    let _sourceBranch = defaultArg sourceBranch ""
    let _sourceCommitHash = defaultArg sourceCommitHash ""

    member _.Critical = _critical
    member _.NonCritical = _nonCritical
    member _.ValidationPackage = _validationPackage
    member _.Payload = _payload
    member _.SourceBranch =
        if _sourceBranch = "" then None else Some _sourceBranch
    member _.SourceCommitHash =
        if _sourceCommitHash = "" then None else Some _sourceCommitHash

    static member create(
        critical: ValidationResult,
        nonCritical: ValidationResult,
        validationPackage: ValidationPackageSummary,
        ?Payload: Json,
        ?SourceBranch: string,
        ?SourceCommitHash: string
    ) =
        ValidationSummary(
            critical,
            nonCritical,
            validationPackage,
            Payload,
            ?sourceBranch = SourceBranch,
            ?sourceCommitHash = SourceCommitHash
        )

    static member toJson(summary: ValidationSummary) =
        let encoder (value: ValidationSummary) =
            Encode.object [
                "Critical", ValidationSummaryCodec.encodeResult value.Critical
                "NonCritical", ValidationSummaryCodec.encodeResult value.NonCritical
                "ValidationPackage", ValidationSummaryCodec.encodePackage value.ValidationPackage

                match value.Payload with
                | Some payload -> "Payload", Encode.value payload
                | None -> ()

                match value.SourceBranch with
                | Some sourceBranch -> "SourceBranch", Encode.string sourceBranch
                | None -> ()

                match value.SourceCommitHash with
                | Some sourceCommitHash -> "SourceCommitHash", Encode.string sourceCommitHash
                | None -> ()
            ]

        JsonRuntime.encode encoder summary

    static member fromJson(json: string) =
        let decoder: Decoder<ValidationSummary> =
            Decode.object (fun get ->
                ValidationSummary.create(
                    get.Required.Field "Critical" ValidationSummaryCodec.resultDecoder,
                    get.Required.Field "NonCritical" ValidationSummaryCodec.resultDecoder,
                    get.Required.Field "ValidationPackage" ValidationSummaryCodec.packageDecoder,
                    ?Payload = get.Optional.Field "Payload" Decode.value,
                    ?SourceBranch = get.Optional.Field "SourceBranch" Decode.string,
                    ?SourceCommitHash = get.Optional.Field "SourceCommitHash" Decode.string
                )
            )

        match JsonRuntime.decode decoder json with
        | Ok summary -> summary
        | Error message -> invalidArg "json" message

    override this.Equals(other) =
        match other with
        | :? ValidationSummary as summary ->
            this.Critical = summary.Critical
            && this.NonCritical = summary.NonCritical
            && this.ValidationPackage = summary.ValidationPackage
            && this.Payload = summary.Payload
            && this.SourceBranch = summary.SourceBranch
            && this.SourceCommitHash = summary.SourceCommitHash
        | _ -> false

    override this.GetHashCode() =
        hash (
            this.Critical,
            this.NonCritical,
            this.ValidationPackage,
            this.Payload,
            this.SourceBranch,
            this.SourceCommitHash
        )
