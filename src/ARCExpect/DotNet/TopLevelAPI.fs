[<AutoOpen>]
module ARCExpect.TopLevel

open System.Collections.Generic
open System.IO
open Thoth.Json.Core

type Execute with

    static member Validation(
        arcValidationPackage: ARCValidationPackage,
        Payload: IDictionary<string, obj>,
        ?SourceBranch: string,
        ?SourceCommitHash: string,
        ?Arguments: PackageArguments
    ) =
        Execute.Validation(
            arcValidationPackage,
            Payload = DotNetPayload.dictionary Payload,
            ?SourceBranch = SourceBranch,
            ?SourceCommitHash = SourceCommitHash,
            ?Arguments = Arguments
        )

    static member SummaryCreation(path: string) =
        fun (summary: ValidationSummary) ->
            File.WriteAllText(path, ValidationSummary.toJson(summary))

    static member JUnitReportCreation(
        path: string,
        ?Verbose: bool
    ) =
        fun (summary: ValidationSummary) ->
            let outputs =
                PipelineOutputBundle.create(
                    summary,
                    VerboseJUnit = defaultArg Verbose false
                )

            File.WriteAllText(path, outputs.JUnitXml)

    static member BadgeCreation(
        path: string,
        labelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Badge.Color>,
        ?DefaultColor: Badge.Color
    ) =
        fun (summary: ValidationSummary) ->
            let portableThresholds =
                Thresholds
                |> Option.map (fun thresholds ->
                    thresholds
                    |> Map.toArray
                    |> Array.map (fun (value, color) ->
                        Badge.Threshold.create(value, color)
                    )
                )

            let outputs =
                PipelineOutputBundle.create(
                    summary,
                    BadgeLabelText = labelText,
                    ?ValueSuffix = ValueSuffix,
                    ?Thresholds = portableThresholds,
                    ?DefaultColor = DefaultColor
                )

            File.WriteAllText(path, outputs.BadgeSvg)

    static member private RunValidationPipeline(
        arcValidationPackage: ARCValidationPackage,
        basePath: string,
        ?Arguments: PackageArguments,
        ?BadgeLabelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Badge.Color>,
        ?DefaultColor: Badge.Color,
        ?VerboseJUnit: bool,
        ?Payload: Json,
        ?SourceBranch: string,
        ?SourceCommitHash: string
    ) =
        let portableThresholds =
            Thresholds
            |> Option.map (fun thresholds ->
                thresholds
                |> Map.toArray
                |> Array.map (fun (value, color) ->
                    Badge.Threshold.create(value, color)
                )
            )

        let summary =
            Execute.Validation(
                arcValidationPackage,
                ?Payload = Payload,
                ?SourceBranch = SourceBranch,
                ?SourceCommitHash = SourceCommitHash,
                ?Arguments = Arguments
            )
            |> Async.RunSynchronously

        let outputs =
            PipelineOutputBundle.create(
                summary,
                ?BadgeLabelText = BadgeLabelText,
                ?ValueSuffix = ValueSuffix,
                ?Thresholds = portableThresholds,
                ?DefaultColor = DefaultColor,
                ?VerboseJUnit = VerboseJUnit
            )

        OutputFileSystem.write basePath outputs

    static member ValidationPipeline(
        basePath: string,
        ?BadgeLabelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Badge.Color>,
        ?DefaultColor: Badge.Color,
        ?VerboseJUnit: bool,
        ?Payload: Json,
        ?SourceBranch: string,
        ?SourceCommitHash: string,
        ?Arguments: PackageArguments
    ) =
        fun (arcValidationPackage: ARCValidationPackage) ->
            Execute.RunValidationPipeline(
                arcValidationPackage,
                basePath,
                ?Arguments = Arguments,
                ?BadgeLabelText = BadgeLabelText,
                ?ValueSuffix = ValueSuffix,
                ?Thresholds = Thresholds,
                ?DefaultColor = DefaultColor,
                ?VerboseJUnit = VerboseJUnit,
                ?Payload = Payload,
                ?SourceBranch = SourceBranch,
                ?SourceCommitHash = SourceCommitHash
            )

    static member ValidationPipeline(
        arguments: PackageArguments,
        ?BadgeLabelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Badge.Color>,
        ?DefaultColor: Badge.Color,
        ?VerboseJUnit: bool,
        ?Payload: Json,
        ?SourceBranch: string,
        ?SourceCommitHash: string
    ) =
        Execute.ValidationPipeline(
            arguments.OutputDirectory,
            ?BadgeLabelText = BadgeLabelText,
            ?ValueSuffix = ValueSuffix,
            ?Thresholds = Thresholds,
            ?DefaultColor = DefaultColor,
            ?VerboseJUnit = VerboseJUnit,
            ?Payload = Payload,
            ?SourceBranch = SourceBranch,
            ?SourceCommitHash = SourceCommitHash,
            Arguments = arguments
        )

    static member ValidationPipeline(
        basePath: string,
        Payload: IDictionary<string, obj>,
        ?BadgeLabelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Badge.Color>,
        ?DefaultColor: Badge.Color,
        ?VerboseJUnit: bool,
        ?SourceBranch: string,
        ?SourceCommitHash: string,
        ?Arguments: PackageArguments
    ) =
        Execute.ValidationPipeline(
            basePath,
            ?BadgeLabelText = BadgeLabelText,
            ?ValueSuffix = ValueSuffix,
            ?Thresholds = Thresholds,
            ?DefaultColor = DefaultColor,
            ?VerboseJUnit = VerboseJUnit,
            Payload = DotNetPayload.dictionary Payload,
            ?SourceBranch = SourceBranch,
            ?SourceCommitHash = SourceCommitHash,
            ?Arguments = Arguments
        )

    static member ValidationPipeline(
        arguments: PackageArguments,
        Payload: IDictionary<string, obj>,
        ?BadgeLabelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Badge.Color>,
        ?DefaultColor: Badge.Color,
        ?VerboseJUnit: bool,
        ?SourceBranch: string,
        ?SourceCommitHash: string
    ) =
        Execute.ValidationPipeline(
            arguments.OutputDirectory,
            Payload,
            ?BadgeLabelText = BadgeLabelText,
            ?ValueSuffix = ValueSuffix,
            ?Thresholds = Thresholds,
            ?DefaultColor = DefaultColor,
            ?VerboseJUnit = VerboseJUnit,
            ?SourceBranch = SourceBranch,
            ?SourceCommitHash = SourceCommitHash,
            Arguments = arguments
        )
