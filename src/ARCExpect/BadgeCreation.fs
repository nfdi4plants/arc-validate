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


    static member ofValidationSummary(
        labelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color
    ) =
        
        fun (validationSummary: ValidationSummary) ->

            let total = validationSummary.Critical.Total + validationSummary.NonCritical.Total

            let totalPassed = validationSummary.Critical.Passed + validationSummary.NonCritical.Passed

            let criticalFailedOrErrored = validationSummary.Critical.Failed + validationSummary.Critical.Errored

            if validationSummary.Critical.HasFailures then

                Badge(
                    label = labelText,
                    defaultColor = Color.RED,
                    value = criticalFailedOrErrored,
                    ValueSuffix = $" Critical Errors"
                )

            else

                let thresholds = 
                    Thresholds
                    |> Option.defaultValue (Map([
                        0, Color.RED
                        total/2, Color.ORANGE_2
                        total, Color.GREEN
                    ]))

                let valueSuffix = 
                    ValueSuffix
                    |> Option.defaultValue $"/{total}"

                Badge(
                    label = labelText,
                    defaultColor = (DefaultColor |> Option.defaultValue (Color.fromString Defaults.DEFAULT_COLOR) ),
                    Thresholds = thresholds,
                    value = totalPassed,
                    ValueSuffix = valueSuffix
                )