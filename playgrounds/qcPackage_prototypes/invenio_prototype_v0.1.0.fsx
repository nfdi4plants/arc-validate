#I "../../src/arc-validate/bin/Debug/net6.0"
#I "../../src/arc-validate/bin/Release/net6.0"
#r "ARCExpect.dll"
#r "ARCTokenization.dll"
#r "ControlledVocabulary.dll"
#r "arc-validate.dll"
#r "Anybadge.NET.dll"
#r "Expecto.dll"
#r "FsOboParser.dll"

//#r "nuget: ARCExpect"
#r "nuget: Anybadge.NET"
#r "nuget: FSharpAux"
#r "nuget: Graphoscope"

open ARCExpect
open ARCTokenization
open ARCTokenization.StructuralOntology
open ControlledVocabulary
open Expecto
open Expecto.Tests
open AnyBadge.NET
open ARCValidate
open FSharpAux
open Graphoscope

open System.IO


// input:

let arcDir = @"C:\Repos\git.nfdi4plants.org\ArcPrototype"
let outDir = arcDir

// Tokenization:

// <<<<<<<<<<<<<<<<<
// Helper Functions:

let parseIsaMetadataSheetFromCvp isaFileName isaMdsParsingF absFileTokens =
    absFileTokens
    |> Seq.choose (
        fun cvp ->
            let cvpStr = Param.getValueAsString cvp
            //printfn $"cvpStr: {cvpStr}"
            if String.contains isaFileName cvpStr then
                Some (isaMdsParsingF cvpStr)
            else None
    )

let parseInvestigationMetadataSheetFromCvp absFileTokens =
    parseIsaMetadataSheetFromCvp "isa.investigation.xlsx" ARCTokenization.Investigation.parseMetadataSheetFromFile absFileTokens

let parseStudyMetadataSheetFromCvp absFileTokens =
    parseIsaMetadataSheetFromCvp "isa.study.xlsx" ARCTokenization.Study.parseMetadataSheetfromFile absFileTokens

let parseAssayMetadataSheetFromCvp absFileTokens =
    parseIsaMetadataSheetFromCvp "isa.assay.xlsx" ARCTokenization.Assay.parseMetadataSheetFromFile absFileTokens

let tryParseIsaMetadataSheetFromCvp isaFileName isaMdsParsingF absFileTokens =
    absFileTokens
    |> Seq.choose (
        fun cvp ->
            let cvpStr = Param.getValueAsString cvp
            //printfn $"cvpStr: {cvpStr}"
            if String.contains isaFileName cvpStr then
                try Some (isaMdsParsingF cvpStr)
                with _ -> None
            else None
    )

let tryParseInvestigationMetadataSheetFromCvp absFileTokens =
    try tryParseIsaMetadataSheetFromCvp "isa.investigation.xlsx" ARCTokenization.Investigation.parseMetadataSheetFromFile absFileTokens 
        |> Seq.concat
    with _ -> Seq.empty

let tryParseStudyMetadataSheetFromCvp absFileTokens =
    tryParseIsaMetadataSheetFromCvp "isa.study.xlsx" ARCTokenization.Study.parseMetadataSheetfromFile absFileTokens

let tryParseAssayMetadataSheetFromCvp absFileTokens =
    tryParseIsaMetadataSheetFromCvp "isa.assay.xlsx" ARCTokenization.Assay.parseMetadataSheetFromFile absFileTokens

let fillTokenList onto tokens =
    let ontoGraph = OboGraph.ontologyToFGraphByName onto
    let ipsAdded = ARCGraph.addMissingTerms onto tokens
    let partitionedIps = Seq.groupWhen (ARCGraph.isHeader ontoGraph) ipsAdded
    let partitionallyFilledIps = partitionedIps |> Seq.map (ARCGraph.addMissingTermsInGroup ontoGraph)
    let groupedIps = partitionallyFilledIps |> Seq.map ARCGraph.groupTerms
    let matchedIps = groupedIps |> Seq.map (ARCGraph.matchTerms onto)
    let subgraphs = Seq.map (ARCGraph.constructIntermediateMetadataSubgraph ontoGraph) matchedIps
    let filledSubgraphs = Seq.map (fst >> ARCGraph.addEmptyIpsToNodeData) subgraphs
    let splitSubgraphs = Seq.map ARCGraph.splitMetadataSubgraph filledSubgraphs
    Seq.map ARCGraph.metadataSubgraphToList splitSubgraphs

// End of Helper Functions:
// >>>>>>>>>>>>>>>>>>>>>>>>

let fsTokensRelDirs = ARCTokenization.FileSystem.parseRelativeDirectoryPaths arcDir
let fsTokensAbsDirs = ARCTokenization.FileSystem.parseAbsoluteDirectoryPaths arcDir
let fsTokensRelFils = ARCTokenization.FileSystem.parseAbsoluteFilePaths arcDir
let fsTokensAbsFils = ARCTokenization.FileSystem.parseAbsoluteFilePaths arcDir

let invesTokens = tryParseInvestigationMetadataSheetFromCvp fsTokensAbsFils
let studyTokens = tryParseStudyMetadataSheetFromCvp fsTokensAbsFils
let assayTokens = tryParseAssayMetadataSheetFromCvp fsTokensAbsFils

let filledInvesTokens = fillTokenList Terms.InvestigationMetadata.ontology invesTokens
let filledStudyTokens = studyTokens |> Seq.map (fillTokenList Terms.StudyMetadata.ontology)
let filledAssayTokens = assayTokens |> Seq.map (fillTokenList Terms.AssayMetadata.ontology)

// Validation cases:

let invesCases = testList "" [
    testList INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name (
        invesTokens 
        |> Seq.filter (fun ip -> ip.Name = "Investigation Person First Name")
        |> Seq.map (
            fun ip ->
                let thisTest = test "is not empty"
                thisTest.Run (fun _ ->
                    try ip :?> CvParam |> ARCExpect.ByValue.notEmpty
                    with :? System.InvalidCastException -> ()
                )
        )
        |> Seq.toList
    )
    testList INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name (
        invesTokens 
        |> Seq.filter (fun ip -> ip.Name = "Investigation Person Last Name")
        |> Seq.map (
            fun ip ->
                let thisTest = test "is not empty"
                thisTest.Run (fun _ ->
                    try ip :?> CvParam |> ARCExpect.ByValue.notEmpty
                    with :? System.InvalidCastException -> ()
                )
        )
        |> Seq.toList
    )
    testList INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name (
        invesTokens 
        |> Seq.filter (fun ip -> ip.Name = "Investigation Person Last Name")
        |> Seq.map (
            fun ip ->
                let thisTest = test "is not empty"
                thisTest.Run (fun _ ->
                    try ip :?> CvParam |> ARCExpect.ByValue.notEmpty
                    with :? System.InvalidCastException -> ()
                )
        )
        |> Seq.toList
    )
    testList INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name (
        invesTokens 
        |> Seq.filter (fun ip -> ip.Name = "Investigation Person Affiliation")
        |> Seq.map (
            fun ip ->
                let thisTest = test "is not empty"
                thisTest.Run (fun _ ->
                    try ip :?> CvParam |> ARCExpect.ByValue.notEmpty
                    with :? System.InvalidCastException -> ()
                )
        )
        |> Seq.toList
    )
    testList INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name (
        invesTokens 
        |> Seq.filter (fun ip -> ip.Name = "Investigation Person Email")
        |> Seq.map (
            fun ip ->
                let thisTest = test "is not empty"
                thisTest.Run (fun _ ->
                    try ip :?> CvParam |> ARCExpect.Valid.email
                    with :? System.InvalidCastException -> ()
                )
        )
        |> Seq.toList
    )
]


// <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
// Temporarily copypasted vom ValidateAPI.fs:

let validate (verbose: bool) arcDir outDir =

    let root = arcDir
    //    args.TryGetResult(ARC_Directory)
    //    |> Option.defaultValue (System.Environment.GetEnvironmentVariable("ARC_PATH")) // default to ARC_PATH if argument is not provided
    //    |> fun s -> if System.String.IsNullOrWhiteSpace(s) then System.Environment.CurrentDirectory else s // default to ./ if ARC_PATH is not set
    //    |> Path.GetFullPath
    //    |> fun p -> p.Replace("\\","/")
    //    |> fun p -> if not (p.EndsWith("/")) then p + "/" else p // ensure path ends with a slash

    let outPath = outDir
        //args.TryGetResult(Out_Directory)
        //|> Option.defaultValue root

    /// these tests MUST pass for an ARC to be considered for publishing
    let criticalTests =
        testList "Critical" [
            invesCases
        ]

    let criticalTestResults =
        criticalTests
        |> performTest

    if criticalTestResults.failed |> List.isEmpty && criticalTestResults.errored |> List.isEmpty then // if no critical tests failed or errored
            
        /// these tests SHOULD pass for an ARC to be considered of high quality
        let nonCriticalTests =
            testList "Non-critical" [
                //TestGeneration.NonCritical.ARC.ISA.generateISATests investigationTokens   // atm. not wanted
            ]

        let nonCriticalTestResults =
            nonCriticalTests
            |> performTest

        let combinedTestResults = 
            [criticalTestResults; nonCriticalTestResults] 
            |> combineTestRunSummaries // aggregate critical and non-critical test results

        let badge = 
            combinedTestResults
            |> BadgeCreation.createSuccessBadge "ARC quality"
            
        badge.WriteBadge(Path.Combine(outPath, "arc-quality.svg"))

        combinedTestResults
        |> writeJUnitSummary verbose (Path.Combine(outPath, "arc-validate-results.xml")) // write the combined result to a single file

        ExitCode.Success // critical tests passed, non-critical tests have been performed. Success!

    else // one or more critical tests failed or errored.

        let badge = 
            criticalTestResults
            |> BadgeCreation.createCriticalFailBadge "ARC quality"
            
        badge.WriteBadge(Path.Combine(outPath, "arc-quality.svg"))

        criticalTestResults
        |> Expecto.writeJUnitSummary verbose (Path.Combine(outPath, "arc-validate-results.xml"))

        ExitCode.CriticalTestFailure

// End of Copypaste
// >>>>>>>>>>>>>>>>


validate true @"C:\Repos\git.nfdi4plants.org\ArcPrototype" @"C:\Repos\git.nfdi4plants.org\ArcPrototype"