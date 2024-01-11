namespace ARCExpect

open AnyBadge.NET

open Expecto

type BadgeCreation =

    static member ofTestResults(
        labelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color
    ) =
        
        fun (testResults: Impl.TestRunSummary) ->

            let max = testResults.passed.Length + testResults.failed.Length

            let thresholds = 
                Thresholds
                |> Option.defaultValue (Map([
                    0, Color.RED
                    max/2, Color.ORANGE_2
                    max, Color.GREEN
                ]))

            let valueSuffix = 
                ValueSuffix
                |> Option.defaultValue $"/{max}"

            Badge(
                label = labelText,
                defaultColor = (DefaultColor |> Option.defaultValue (Color.fromString Defaults.DEFAULT_COLOR) ),
                Thresholds = thresholds,
                value = testResults.passed.Length,
                ValueSuffix = $"/{max}"
            )
