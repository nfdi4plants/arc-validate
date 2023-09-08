namespace ARCValidate.API

open ARCValidate
open ARCValidate.CLIArguments
open ArcValidation
open ArcValidation.Configs

open Expecto
open System.IO
open Argu

module ValidateAPI = 

    let validate (verbose: bool) (args: ParseResults<ValidateArgs>)=

        let arcConfig = 
            args.TryGetResult(ARC_Directory)
            |> Option.defaultValue (System.Environment.GetEnvironmentVariable("ARC_PATH")) // default to ARC_PATH if argument is not provided
            |> fun s -> if System.String.IsNullOrWhiteSpace(s) then System.Environment.CurrentDirectory else s // default to ./ if ARC_PATH is not set
            |> fun s -> ArcConfig(s)

        let outPath = 
            args.TryGetResult(Out_Directory)
            |> Option.defaultValue arcConfig.PathConfig.ArcRootPath

        /// these tests MUST pass for an ARC to be considered for publishing
        let criticalTests =
            testList "Critical" [
                TestGeneration.Critical.Arc.FileSystem.generateArcFileSystemTests arcConfig
                TestGeneration.Critical.Arc.ISA.generateISATests arcConfig
            ]

        let criticalTestResults =
            criticalTests
            |> performTest

        if criticalTestResults.failed |> List.isEmpty && criticalTestResults.errored |> List.isEmpty then // if no critical tests failed or errored
            
            /// these tests SHOULD pass for an ARC to be considered of high quality
            let nonCriticalTests =
                testList "Non-critical" [
                    TestGeneration.NonCritical.Arc.ISA.generateISATests arcConfig
                ]

            let nonCriticalTestResults =
                nonCriticalTests
                |> performTest

            [criticalTestResults; nonCriticalTestResults] 
            |> combineTestRunSummaries // aggregate critical and non-critical test results
            |> writeJUnitSummary verbose (Path.Combine(outPath, "arc-validate-results.xml")) // write the combined result to a single file

            ExitCode.Success // critical tests passed, non-critical tests have been performed. Success!

        else // one or more critical tests failed or errored.
            criticalTestResults
            |> Expecto.writeJUnitSummary verbose (Path.Combine(outPath, "arc-validate-results.xml"))

            ExitCode.CriticalTestFailure