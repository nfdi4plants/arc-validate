#I "../ARCTokenization/src/ARCTokenization/bin/Debug/netstandard2.0"
#I "../ARCTokenization/src/ARCTokenization/bin/Release/netstandard2.0"
#r "ARCTokenization.dll"
#r "ControlledVocabulary.dll"
#I "src/ARCExpect/bin/Debug/netstandard2.0"
#I "src/ARCExpect/bin/Release/netstandard2.0"
#r "ARCExpect.dll"

//#r "nuget: ARCTokenization"
#r "nuget: Expecto"
#r "nuget: FSharpAux, 1.1.0"
#r "nuget: Graphoscope"
#r "nuget: Cytoscape.NET"
#r "nuget: FsOboParser, 0.3.0"
#r "nuget: FsSpreadsheet.ExcelIO, 4.1.0"


open Expecto
open ControlledVocabulary
open ARCTokenization
open FSharpAux
//open ArcValidation.OntologyHelperFunctions
//open ArcValidation.ErrorMessage
open Graphoscope
open FsOboParser
open Cytoscape.NET

open ARCExpect
open ARCExpect.OboGraph
open ARCExpect.ARCGraph
open ARCExpect.ARCGraph.Visualization

open System.Collections.Generic
open System.Text.RegularExpressions


// !!!!!!!!!!SEHR GUT!!!!!!!!!!!!
module ArcExpect =
// alternativ: Expect.ARC.isNonEmpty usw.

    // davon werden wir vllt. 10 Stück oder so brauchen
    let hasMetadataSectionKey (arcValidateContext : ARCValidateContext) testName key =
        match Dictionary.tryFind key arcValidateContext.Tokens with
        | Some value -> 
            try 
                getMetadataSectionKey value |> ignore
                ARCValidateContext.addTestCondition testName true arcValidateContext
            with
                | _ -> 
                    ARCValidateContext.addTestCondition testName false arcValidateContext
                    failtestf "%s" (createErrorStack arcValidateContext.Filepath)
        | None -> 
            ARCValidateContext.addTestCondition testName false arcValidateContext
            failtestf "%s" (createErrorStack arcValidateContext.Filepath)

    /// 
    let hasValues (arcValidateContext : ARCValidateContext) testName key =
        match Dictionary.tryFind key arcValidateContext.Tokens with
        | Some value -> 
            let mdsk = getMetadataSectionKey value
            let row = (mdsk :?> CvParam).GetAttribute(Address.row) |> Param.getValueAsInt
            let col = ((mdsk :?> CvParam).GetAttribute(Address.column) |> Param.getValueAsInt) + 1
            let sheet = (mdsk :?> CvParam).GetAttribute(Address.worksheet) |> Param.getValueAsString
            //let message = Message.Create(invPath, XLSXFileKind, row, col, sheet)
            value       // hier muss das filtern noch raus, das soll bereits vorher passieren
            |> List.filter (fun ip -> Param.getValueAsString ip <> (Terms.StructuralTerms.metadataSectionKey |> CvTerm.getName))
            |> fun res ->
                match res with
                | [] ->
                    ARCValidateContext.addTestCondition testName false arcValidateContext
                    failtestf "%s" (createErrorStackWithCell invPath sheet row col)
                | _ -> ARCValidateContext.addTestCondition testName true arcValidateContext
        | None -> 
            ARCValidateContext.addTestCondition testName false arcValidateContext
            failtestf "%s" (createErrorStack arcValidateContext.Filepath)

    let hasAllMetadataSectionKeys (arcValidateContext : ARCValidateContext) testName keyList =
        keyList
        |> List.iter (hasMetadataSectionKey arcValidateContext testName)



let tl = 
    testSequenced (
        testList "Critical" [
            let myArcContext = ARCValidateContext.create invDict (Dictionary()) invPath
            let areAllMetadataSectionKeysPresentTest =
                ARCTest.Create(
                    error = Error.MissingEntity.MissingMetadataKey.name,
                    position = InvestigationMetadata.name,
                    arcValidateContext = myArcContext,
                    test = ArcExpect.hasMetadataSectionKey
                )
            let hasMetadataSectionKeyTest = 
                ARCTest.Create(
                    error = Error.MissingEntity.MissingMetadataKey.name,
                    position = InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name,
                    arcValidateContext = myArcContext,
                    test = ArcExpect.hasMetadataSectionKey
                )
            let hasValuesTest =
                ARCTest.CreateDependent(
                    error = Error.MissingEntity.MissingValue.name,
                    position = InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name,
                    arcValidateContext = myArcContext,
                    dependsOnTest = hasMetadataSectionKeyTest.Name,
                    test = ArcExpect.hasValues
                )
            areAllMetadataSectionKeysPresentTest.Test
            hasMetadataSectionKeyTest.Test
            hasValuesTest.Test
        ]
    )

tl |> performTest

    //testCase $"{Error.MissingEntity.MissingValue.name} test: {InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name}" <| fun () -> 
    //    ArcExpect.isNotEmpty invDict "Investigation Person First Name"













    //let exists

Error.MissingEntity.MissingValue.name

testList "Critical" [
    //testCase 
    testCaseArc 
        Error.MissingEntity.MissingValue.name 
        InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name 

        ArcExpect.isNotEmpty
        //ArcExpect.isNotEmpty invDict "Investigation Person First Name"
]
|> performTest
// !!!!!!!!!!!!!!!!!!!!!!!!!

ArcExpect.isNonEmpty invDict "Investigation Person First Name"


// ValidationResult nicht notwendig, stattdessen alles in Expecto-Funktion evaluieren
// MetadataSection aus dem Dictionary raus
// Addresse bekommen in eine schmale Funktion / zu einer Funktion machen
// createErrorStack-Funktion soll CvParam, Pfad und ErrorOntologyTermName als Parameter bekommen und daraus Fehlermeldung zurückgeben
// "wrong format" fehlt noch in der ErrorOntology
// Expectos Expect.blabla Funktionen alle für uns so schreiben, dass es von der Message her passt

let hasPersonFirstNames = 
    if Dictionary.containsKey "Investigation Person First Name" invDict then
        invDict["Investigation Person First Name"]
        |> fun ipl -> 
            let values =
                ipl
                |> List.filter (fun ip -> Param.getValueAsString ip <> (Terms.StructuralTerms.metadataSectionKey |> CvTerm.getName))
            let check = List.isEmpty values |> not
            if check then
                Success
            else
                let mdsk = getMetadataSectionKey ipl
                let row = (mdsk :?> CvParam).GetAttribute(Address.row) |> Param.getValueAsInt
                let col = ((mdsk :?> CvParam).GetAttribute(Address.column) |> Param.getValueAsInt) + 1
                let sheet = (mdsk :?> CvParam).GetAttribute(Address.worksheet) |> Param.getValueAsString
                let message = Message.Create(invPath, XLSXFileKind, row, col, sheet)
                Error message
    else Error (Message.Create(invPath, XLSXFileKind, 0, 0, ""))
    |> fun res -> 
        testCase InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name (fun _ ->
            res
            |> throwError (
                fun m -> 
                    createErrorStackXlsxFile 
                        m 
                        InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name 
                        Error.MissingEntity.MissingValue.name
            )
        )



let case = 
    testCase InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name (fun _ ->
        hasPersonFirstNames
        |> throwError (
            fun m -> 
                createErrorStackXlsxFile 
                    m 
                    InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name 
                    Error.MissingEntity.MissingValue.name
        )
    )




case |> performTest

//let ups = inv |> List.choose UserParam.tryUserParam
//let cvpsEmptyVals = inv |> List.choose CvParam.tryCvParam |> List.filter (Param.getValueAsString >> (=) "")

//let inv = ARCTokenization.Investigation.parseMetadataRowsFromFile @"C:/Repos/gitlab.nfdi4plants.org/ArcPrototype/isa.investigation.xlsx"

//inv[20]

//Param.getValueAsTerm (CvParam("1", "2", "3", ParamValue.Value ""))
//Param.getValueAsString (CvParam("1", "2", "3", ParamValue.Value ""))
//Param.getValueAsString (CvParam("1", "2", "3", ParamValue.CvValue ("1", "lemmip", "3")))
//Param.getValueAsTerm (CvParam("1", "2", "3", ParamValue.CvValue ("1", "lemmip", "3")))