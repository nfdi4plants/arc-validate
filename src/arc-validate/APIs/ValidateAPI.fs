namespace ARCValidate.API

open ARCValidate
open ARCValidate.CLIArguments
open ARCExpect
open ARCExpect.Configs
open ARCTokenization

open Expecto
open System.IO
open Argu

open ControlledVocabulary
open AnyBadge.NET

module ValidateAPI = 

    let validate (verbose: bool) (args: ParseResults<ValidateArgs>)=

        let root = 
            args.TryGetResult(ARC_Directory)
            |> Option.defaultValue (System.Environment.GetEnvironmentVariable("ARC_PATH")) // default to ARC_PATH if argument is not provided
            |> fun s -> if System.String.IsNullOrWhiteSpace(s) then System.Environment.CurrentDirectory else s // default to ./ if ARC_PATH is not set
            |> Path.GetFullPath
            |> fun p -> p.Replace("\\","/")
            |> fun p -> if not (p.EndsWith("/")) then p + "/" else p // ensure path ends with a slash

        let outPath = 
            args.TryGetResult(Out_Directory)
            |> Option.defaultValue root

        let hasInvFile = File.Exists(Path.Combine(root, "isa.investigation.xlsx"))

        let investigationTokens = 
            if hasInvFile then 
                (Path.Combine(root, "isa.investigation.xlsx"))
                |> Investigation.parseMetadataSheetFromFile()
                |> List.filter (fun p ->
                    Param.getValueAsTerm p <> Terms.StructuralTerms.metadataSectionKey // filter these out to get only value-holding cells
                )
            else 
                []

        /// these tests MUST pass for an ARC to be considered for publishing
        let criticalTests =
            testList "Critical" [
                TestGeneration.Critical.ARC.FileSystem.generateARCFileSystemTests root
                TestGeneration.Critical.ARC.ISA.generateISATests investigationTokens
            ]

        let criticalTestResults = ARCExpect.Execute.Validation criticalTests

        if criticalTestResults.failed |> List.isEmpty && criticalTestResults.errored |> List.isEmpty then // if no critical tests failed or errored
            
            /// these tests SHOULD pass for an ARC to be considered of high quality
            let nonCriticalTests =
                testList "Non-critical" [
                    TestGeneration.NonCritical.ARC.ISA.generateISATests investigationTokens
                ]

            let nonCriticalTestResults = ARCExpect.Execute.Validation nonCriticalTests

            let combinedTestResults = 
                [criticalTestResults; nonCriticalTestResults] 
                |> combineTestRunSummaries // aggregate critical and non-critical test results

            combinedTestResults
            |> Execute.BadgeCreation(
                path = (Path.Combine(outPath, "arc-quality.svg")),
                labelText = "ARC quality"
            )
            
            combinedTestResults
            |> Execute.JUnitSummaryCreation(
                path = (Path.Combine(outPath, "arc-validate-results.xml")),
                Verbose = verbose
            )

            ExitCode.Success // critical tests passed, non-critical tests have been performed. Success!

        else // one or more critical tests failed or errored.

            let badge = 
                criticalTestResults
                |> BadgeCreation.createCriticalFailBadge "ARC quality"
            
            badge.WriteBadge(Path.Combine(outPath, "arc-quality.svg"))

            let testAmnt = criticalTestResults.failed.Length + criticalTestResults.errored.Length

            criticalTestResults
            |> Execute.BadgeCreation(
                path = (Path.Combine(outPath, "arc-quality.svg")),
                labelText = "ARC quality",
                ValueSuffix = $"/{testAmnt} critical tests passed",
                DefaultColor = Color.RED
            )

            criticalTestResults
            |> Execute.JUnitSummaryCreation(
                path = (Path.Combine(outPath, "arc-validate-results.xml")),
                Verbose = verbose
            )

            ExitCode.CriticalTestFailure