#I "../../src/arc-validate/bin/Debug/net8.0"
#I "../../src/arc-validate/bin/Release/net8.0"
#r "ARCExpect.dll"
#r "ARCTokenization.dll"
#r "ControlledVocabulary.dll"
#r "arc-validate.dll"
#r "Anybadge.NET.dll"
#r "Expecto.dll"
#r "OBO.NET.dll"

//#r "nuget: ARCExpect"
#r "nuget: Anybadge.NET"
#r "nuget: FSharpAux"
#r "nuget: Graphoscope"
#r "nuget: Cytoscape.NET"

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
open Cytoscape

open System.IO


// input:

let arcDir = @"C:\Repos\git.nfdi4plants.org\ArcPrototype"
let outDir = arcDir

// Tokenization:

// <<<<<<<<<<<<<<<<<
// Helper Functions:

// <--- into ARCTokenization:
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

let tryParseIsaMetadataSheetFromCvp (isaFileName : string) isaMdsParsingF absFileTokens =
    absFileTokens
    |> Seq.choose (
        fun cvp ->
            let cvpStr = Param.getValueAsString cvp
            //printfn $"cvpStr: {cvpStr}"
            if String.contains isaFileName (Path.GetFileName cvpStr) then
                try Some (isaMdsParsingF cvpStr)
                with _ -> None
            else None
    )

let tryParseInvestigationMetadataSheetFromCvp (absFileTokens : #IParam seq) =
    try tryParseIsaMetadataSheetFromCvp "isa.investigation.xlsx" ARCTokenization.Investigation.parseMetadataSheetFromFile absFileTokens 
        |> Seq.concat
    with _ -> Seq.empty

let tryParseStudyMetadataSheetFromCvp (absFileTokens : #IParam seq) =
    tryParseIsaMetadataSheetFromCvp "isa.study.xlsx" ARCTokenization.Study.parseMetadataSheetfromFile absFileTokens

let tryParseAssayMetadataSheetFromCvp (absFileTokens : #IParam seq) =
    tryParseIsaMetadataSheetFromCvp "isa.assay.xlsx" ARCTokenization.Assay.parseMetadataSheetFromFile absFileTokens
// --->

let mockInv = 
    ARCMock.InvestigationMetadataTokens(
        Investigation_Identifier = ["ArcPrototype"],
        Investigation_Title = ["ArcPrototype"],
        Investigation_Description = ["A prototypic ARC that implements all specification standards accordingly"],
        Investigation_Person_Last_Name = ["Mühlhaus"; "Garth"; "Maus"],
        Investigation_Person_First_Name = ["Timo"; "Christoph"; "Oliver";],
        Investigation_Person_Email = ["timo.muehlhaus@rptu.de"; "garth@rptu.de"; "maus@nfdi4plants.org"],
        Investigation_Person_Affiliation = ["RPTU University of Kaiserslautern"; "RPTU University of Kaiserslautern"; "RPTU University of Kaiserslautern"],
        Investigation_Person_Phone = ["0 49 (0)631 205 4657"],
        Investigation_Person_Address = ["RPTU University of Kaiserslautern, Paul-Ehrlich-Str. 23 , 67663 Kaiserslautern"; ""; "RPTU University of Kaiserslautern, Erwin-Schrödinger-Str. 56 , 67663 Kaiserslautern"],
        Investigation_Person_Roles = ["principal investigator"; "principal investigator"; "research assistant"],
        Investigation_Person_Roles_Term_Source_REF = ["scoro"; "scoro"; "scoro"],
        Investigation_Person_Roles_Term_Accession_Number = ["http://purl.org/spar/scoro/principal-investigator"; "http://purl.org/spar/scoro/principal-investigator"; "http://purl.org/spar/scoro/research-assistant"],
        Comment_ORCID = ["http://orcid.org/0000-0003-3925-6778"; ""; "0000-0002-8241-5300"],
        Study_Identifier = ["experiment1_material"; "experiment2"],
        Study_File_Name = [@"experiment1_material\isa.study.xlsx"; @"experiment2\isa.study.xlsx"],
        Study_Assay_File_Name = [@"measurement1\isa.assay.xlsx"; @"measurement2\isa.assay.xlsx"]
    )
    |> List.concat // use flat list
    |> Seq.map (fun cvp -> cvp :> IParam)

let mockStu =
    ARCMock.StudyMetadataTokens(
        Study_Identifier = ["experiment1_material"],
        Study_Title = ["Prototype for experimental data"],
        Study_Description = ["In this a devised study to have an exemplary experimental material description."],
        Study_File_Name = [@"experiment1_material\isa.study.xlsx"]
    )
    |> List.concat // use flat list
    |> Seq.map (fun cvp -> cvp :> IParam)

let mockAss =
    ARCMock.AssayMetadataTokens(
        Assay_File_Name = [@"measurement1\isa.assay.xlsx"],
        Assay_Performer_Last_Name = ["Maus"; "Katz"],
        Assay_Performer_First_Name = ["Oliver"; "Marius"],
        Assay_Performer_Mid_Initials = [""; "G."],
        Assay_Performer_Email = ["maus@nfdi4plants.org"],
        Assay_Performer_Affiliation = ["RPTU University of Kaiserslautern"],
        Assay_Performer_Roles = ["research assistant"],
        Assay_Performer_Roles_Term_Accession_Number = ["http://purl.org/spar/scoro/research-assistant"],
        Assay_Performer_Roles_Term_Source_REF = ["scoro"]
    )
    |> List.concat // use flat list
    |> Seq.map (fun cvp -> cvp :> IParam)

mockAss |> Seq.iter (fun ip -> ip.Name |> printfn "%s")
mockAss |> Seq.iter (fun ip -> printfn "%s, %s, %s" ip.Name ip.Accession ip.RefUri)

open ARCGraph
let onto = Terms.AssayMetadata.ontology
//let onto = Terms.StudyMetadata.ontology
let ontoGraph = OboGraph.ontologyToFGraphByName onto
let ipsAdded = addMissingTerms onto mockAss
//let ipsAdded = addMissingTerms onto mockStu
ipsAdded |> Seq.iter (fun ip -> printfn "%s, %s, %s" ip.Name ip.Accession ip.RefUri)
let partitionedIps = Seq.groupWhen (isHeader ontoGraph) ipsAdded
partitionedIps |> Seq.iter (fun  l -> printfn "\ns1:"; l |> Seq.iter (fun ip -> printfn "%s, %s, %s" ip.Name ip.Accession ip.RefUri))
let partitionallyFilledIps = partitionedIps |> Seq.map (addMissingTermsInGroup ontoGraph)
partitionallyFilledIps |> Seq.iter (fun  l -> printfn "\ns1:"; l |> Seq.iter (fun ip -> printfn "%s, %s, %s" ip.Name ip.Accession ip.RefUri))
let groupedIps = partitionallyFilledIps |> Seq.map groupTerms
groupedIps |> Seq.iter (fun l -> printfn "\ns1:"; l |> Seq.iter (fun (j,k) -> printfn "s2:"; k |> Seq.iter (fun ip -> printfn "%s, %s, %s" ip.Name ip.Accession ip.RefUri)))
let matchedIps = groupedIps |> Seq.map (matchTerms onto)
matchedIps |> Seq.iter (fun l -> printfn "\ns1:"; l |> Seq.iter (fun (j,k) -> printfn "s2:"; k |> Seq.iter (fun ip -> printfn "%s, %s, %s" (deconstructTf ip).Name (deconstructTf ip).Accession (deconstructTf ip).RefUri)))
let subgraphs = Seq.map (constructIntermediateMetadataSubgraph ontoGraph) matchedIps
subgraphs |> Seq.iter (fst >> ARCGraph.Visualization.isaIntermediateGraphToFullCyGraph >> Cytoscape.NET.CyGraph.show)
let filledSubgraphs = Seq.map (fst >> addEmptyIpsToNodeData) subgraphs
let splitSubgraphs = Seq.map splitMetadataSubgraph filledSubgraphs
Seq.map metadataSubgraphToList splitSubgraphs |> Seq.iter (fun l -> printfn "\ns1:"; l |> Seq.iter (fun k -> printfn "s2:"; k |> Seq.iter (fun ((_,_),ip) -> printfn "%s, %s, %s" ip.Name ip.Accession ip.RefUri)))

let actual = ARCGraph.fillTokenList Terms.AssayMetadata.ontology mockAss
actual |> Seq.length
actual |> Seq.iter (fun s1 -> printfn "\ns1:"; s1 |> Seq.iter (printfn "s2:"; Seq.iter (fun ((s,d),f) -> printfn $"{s}; {f.Value |> ParamValue.getValueAsString}")))
let act1 = actual |> Seq.tryItem 0 |> Option.bind (Seq.tryFind (fun t -> t |> Seq.exists (fun (t1,t2) -> t1 = ("Assay File Name", 1))))
act1 |> Option.defaultValue Seq.empty |> Seq.iter (fun (s,f) -> printfn $"{fst s}; {f.Value |> ParamValue.getValueAsString}")
Expect.isSome act1 "missing Assay File Name"
let act2 = 
    Option.defaultValue Seq.empty act1 
    |> Seq.map (fun (t1,t2) -> t2.Value |> ParamValue.getValueAsString) 
    |> Seq.tryItem 8
    |> Option.defaultValue ""


// End of Helper Functions:
// >>>>>>>>>>>>>>>>>>>>>>>>

// make this a single function

let fsTokensAbsFils = ARCTokenization.FileSystem.parseAbsoluteFilePaths arcDir

let invesTokens = tryParseInvestigationMetadataSheetFromCvp fsTokensAbsFils
let studyTokens = tryParseStudyMetadataSheetFromCvp fsTokensAbsFils
let assayTokens = tryParseAssayMetadataSheetFromCvp fsTokensAbsFils

let filledInvesTokens = fillTokenList Terms.InvestigationMetadata.ontology invesTokens
let filledStudyTokens = studyTokens |> Seq.map (fillTokenList Terms.StudyMetadata.ontology)
let filledAssayTokens = assayTokens |> Seq.map (fillTokenList Terms.AssayMetadata.ontology)

// Validation cases:
//ARCExpect.ARCExpect.ByTerm.forallBy INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name`` ARCExpect.ByValue.notEmpty invesTokens
let forallBy term action ip =
    r

let invesCases = testList "" [
    testList INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name (
        filledInvesTokens 
        |> Seq.filter (fun ip -> ip.Name = "Investigation Person First Name")
        |> Seq.map (
            fun ip ->
                ARCExpect.test (Name "is not empty") {
                    try ip :?> CvParam |> ARCExpect.ByValue.notEmpty
                    with :? System.InvalidCastException -> ()
                }
                //let thisTest = test "is not empty"
                //thisTest.Run (fun _ ->
                //    try ip :?> CvParam |> ARCExpect.ByValue.notEmpty
                //    with :? System.InvalidCastException -> ()
                //)
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
                    try ip :?> CvParam |> Validate.ByValue.notEmpty
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
                    try ip :?> CvParam |> Validate.ByValue.notEmpty
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
                    try ip :?> CvParam |> Validate.ByValue.notEmpty
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
                    try ip :?> CvParam |> Validate.email
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


ARCValidate.API.ValidateAPI.validate

validate true @"C:\Repos\git.nfdi4plants.org\ArcPrototype" @"C:\Repos\git.nfdi4plants.org\ArcPrototype"