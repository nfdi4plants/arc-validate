namespace ARCExpect

open AnyBadge.NET
open Expecto

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