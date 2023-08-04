#I "src/ArcValidation/bin/Debug/netstandard2.0"
//#r "ArcValidation.dll"

#r "nuget: ARCTokenization"
#r "nuget: Expecto"
#r "nuget: FSharpAux"

open Expecto
open ControlledVocabulary
open ARCTokenization
open FSharpAux
//open ArcValidation.OntologyHelperFunctions
//open ArcValidation.ErrorMessage

open System.Collections.Generic


module Error =

    module MissingEntity =

        type MissingValue =
            static member id = "DPEO:00000003"
            static member name = "Missing Value"


module InvestigationMetadata =

    module InvestigationContacts =

        type InvestigationPersonLastName =
            static member id = "INVMSO:00000022"
            static member name = "Investigation Person Last Name"

        type InvestigationPersonFirstName =
            static member id = "INVMSO:00000023"
            static member name = "Investigation Person First Name"




type MessageKind =
    | FilesystemEntryKind
    | TextfileKind
    | XLSXFileKind

type Message = {
        Path        : string
        Line        : int option
        Position    : int option
        Sheet       : string option
        Kind        : MessageKind
    }
    with

    static member create path line pos sheet kind = {Path = path; Line = line; Position = pos; Sheet = sheet; Kind = kind}
    static member Create(path, kind, ?Line, ?Pos, ?Sheet) = {Path = path; Line = Line; Position = Pos; Sheet = Sheet; Kind = kind}

type ValidationResult =
    | Success
    | Error of Message

let throwError failStringFunction result = 
    match result with
    | ValidationResult.Success -> ()
    | ValidationResult.Error m -> failtestf "%s" (failStringFunction m)

open Impl

let performTest test =
    let w = System.Diagnostics.Stopwatch()
    w.Start()
    evalTests Tests.defaultConfig test
    |> Async.RunSynchronously
    |> fun r -> 
        w.Stop()
        {
            results = r
            duration = w.Elapsed
            maxMemory = 0L
            memoryLimit = 0L
            timedOut = []
        }


let createErrorStackFilesystemEntry (message : Message) structuralOntologyTerm errorOntologyTerm =
    $"{errorOntologyTerm} Error: {structuralOntologyTerm}.\nat {message.Path}"

let createErrorStackTextfile (message : Message) structuralOntologyTerm errorOntologyTerm =
    $"{errorOntologyTerm} Error: {structuralOntologyTerm}.\nat {message.Path}"

let createErrorStackXlsxFile (message : Message) structuralOntologyTerm errorOntologyTerm =
    let cellString = FsSpreadsheet.FsAddress(message.Line.Value, message.Position.Value).Address
    $"{errorOntologyTerm} Error: {structuralOntologyTerm}.\nat '{message.Path}' > sheet '{message.Sheet.Value}' > cell '{cellString}'"

//let getRelativePath fullpath =      // alternative names: `getRelativeArcPath`, `getArcRelativePath`, `get
    


let invPath = @"C:/Repos/gitlab.nfdi4plants.org/ArcPrototype/isa.investigation.xlsx"

let inv = ARCTokenization.Investigation.parseMetadataSheetFromFile invPath

let invDict = Dictionary.ofList (inv |> List.groupBy CvBase.getCvName)

let getMetadataSectionKey iParamList = 
    iParamList
    |> List.filter (fun ip -> Param.getValueAsString ip = (Terms.StructuralTerms.metadataSectionKey |> CvTerm.getName))
    |> List.exactlyOne


let actual = true
let actualList = []

testList "meinTest" [
    testCase "meinCase" <| fun () -> Expect.isTrue actual "falsch"
]

testList "meinTest" [
    testCase "meinCase" <| fun () -> Expect.isTrue actual (createErrorStackXlsxFile (Message.Create("pfad", XLSXFileKind, 5, 1, "meinSheet")))
]

testList "meineTests" [
    testCase "email" (fun () ->
        Expect.isEmpty email "Email is empty"
        Expect.isTrue (checkForCorrectFormat email) "Email has incorrect format"
    )
]

let testCaseName = $"{Error.MissingEntity.MissingValue.name} test: {InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name}"
let errorMessage = Message.Create("pfad", XLSXFileKind, 5, 1, "meinSheet")

testCase testCaseName (fun _ -> Expect.isTrue false (errorMessage.ToString()))
|> performTest

testList "meinTest" [

    testCase $"{Error.MissingEntity.MissingValue.name} test: {InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name}" <| fun () -> 
        
        match Dictionary.tryFind "Investigation Person First Name" invDict with
        | Some value -> 
            value
            |> List.filter (fun ip -> Param.getValueAsString ip <> (Terms.StructuralTerms.metadataSectionKey |> CvTerm.getName))
        | None -> []
        |> fun actualList -> 
            Expect.isNonEmpty actualList (Message.Create("pfad", XLSXFileKind, 5, 1, "meinSheet").ToString())
]


// !!!!!!!!!!SEHR GUT!!!!!!!!!!!!
module ArcExpect =

    // davon werden wir vllt. 10 Stück oder so brauchen
    let isNonEmpty (dict : #IDictionary<string,IParam list>) key (message : Message) =
        match Dictionary.tryFind key dict with
        | Some value -> 
            value       // hier muss das filtern noch raus, das soll bereits vorher passieren
            |> List.filter (fun ip -> Param.getValueAsString ip <> (Terms.StructuralTerms.metadataSectionKey |> CvTerm.getName))
        | None -> []
        |> fun res ->
            match res with
            | [] ->
                failtestf "%s" (sprintf "'%s' > sheet '%s' > cell '%s'" message.Path message.Sheet.Value (FsSpreadsheet.FsAddress(message.Line.Value, message.Position.Value).Address))
            | _ -> ()

testList "Critical" [
    testCase $"{Error.MissingEntity.MissingValue.name} test: {InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name}" <| fun () -> 
        ArcExpect.isNonEmpty invDict "Investigation Person First Name" (Message.Create("pfad", XLSXFileKind, 5, 1, "meinSheet"))
]
|> performTest
// !!!!!!!!!!!!!!!!!!!!!!!!!


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
//Param.getValueAsString (CvParam("1", "2", "3", ParamValue.CvValue ("1", "Pimmel", "3")))
//Param.getValueAsTerm (CvParam("1", "2", "3", ParamValue.CvValue ("1", "Pimmel", "3")))