namespace ARCExpect

open AnyBadge.NET
open Expecto
open System.IO

type Execute =
    
    static member Validation (validationCases: Test) = performTest validationCases

    static member JUnitSummaryCreation(
        path: string,
        ?Verbose: bool
    ) =
        let verbose = defaultArg Verbose false
        fun (validationResults: Impl.TestRunSummary) -> writeJUnitSummary verbose path validationResults

    static member BadgeCreation(
        path: string,
        labelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color
    ) =
        fun (validationResults: Impl.TestRunSummary) -> 
            validationResults
            |> BadgeCreation.ofTestResults(
                labelText,
                ?ValueSuffix = ValueSuffix,
                ?Thresholds = Thresholds,
                ?DefaultColor = DefaultColor
            )
            |> fun b -> b.WriteBadge(path)

    static member ValidationPipeline(
        jUnitPath: string,
        badgePath: string,
        labelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color
    ) =
        fun (validationCases: Test) ->

            let results = 
                validationCases
                |> Execute.Validation

            results
            |> Execute.JUnitSummaryCreation(jUnitPath)

            results
            |> Execute.BadgeCreation(badgePath, labelText, ?ValueSuffix = ValueSuffix, ?Thresholds = Thresholds, ?DefaultColor = DefaultColor)

    static member ValidationPipeline(
        basePath: string,
        packageName: string,
        ?BadgeLabelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color
    ) =
        fun (validationCases: Test) ->

            let resultFolder = Path.Combine(basePath, ".arc-validate-results", packageName)
            let badgePath = Path.Combine(resultFolder, "badge.svg")
            let jUnitPath = Path.Combine(resultFolder, "validation_report.xml")

            Directory.CreateDirectory(resultFolder) |> ignore

            let results = 
                validationCases
                |> Execute.Validation

            results
            |> Execute.JUnitSummaryCreation(jUnitPath)

            let labelText = defaultArg BadgeLabelText packageName

            results
            |> Execute.BadgeCreation(badgePath, labelText, ?ValueSuffix = ValueSuffix, ?Thresholds = Thresholds, ?DefaultColor = DefaultColor)