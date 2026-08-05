namespace ARCExpect

open Fable.Core

[<AttachMembers>]
type internal PipelineOutputBundle private (
    folderName: string,
    summaryJson: string,
    junitXml: string,
    badgeSvg: string
) =

    let _folderName = folderName
    let _summaryJson = summaryJson
    let _junitXml = junitXml
    let _badgeSvg = badgeSvg

    member _.FolderName = _folderName
    member _.ResultDirectoryName = ".arc-validate-results"
    member _.SummaryFileName = "validation_summary.json"
    member _.JUnitFileName = "validation_report.xml"
    member _.BadgeFileName = "badge.svg"
    member _.SummaryJson = _summaryJson
    member _.JUnitXml = _junitXml
    member _.BadgeSvg = _badgeSvg

    static member create(
        summary: ValidationSummary,
        ?BadgeLabelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Badge.Threshold array,
        ?DefaultColor: Badge.Color,
        ?VerboseJUnit: bool
    ) =
        let package = summary.ValidationPackage
        let packageIdentity = $"{package.Name}@{package.Version}"
        let combined =
            RunSummary.combine [| summary.Critical; summary.NonCritical |]

        let junitXml =
            JUnit.Writer.toXml(
                combined,
                Verbose = defaultArg VerboseJUnit false,
                SuiteName = package.Name,
                ?SourceBranch = summary.SourceBranch,
                ?SourceCommitHash = summary.SourceCommitHash
            )

        let badgeSvg =
            Badge.Writer.toSvg(
                summary,
                defaultArg BadgeLabelText packageIdentity,
                ?ValueSuffix = ValueSuffix,
                ?Thresholds = Thresholds,
                ?DefaultColor = DefaultColor
            )

        PipelineOutputBundle(
            packageIdentity,
            ValidationSummary.toJson(summary),
            junitXml,
            badgeSvg
        )
