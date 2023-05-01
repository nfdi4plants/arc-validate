#r "nuget: FSharpAux.Core,2.0.0"
#r "nuget: FsSpreadsheet.ExcelIO"
#r "nuget: ArcGraphModel, 0.1.0-preview.1"
#r "nuget: ArcGraphModel.IO, 0.1.0-preview.1"
#I "src/bin/Debug/net6.0"
#r "arc-validate.dll"

//#I @"src\ArcGraphModel\bin\Release\net6.0"
//#I @"../ArcGraphModel/src\ArcGraphModel.IO\bin\Release\net6.0"
//#I @"../ArcGraphModel/src\ArcGraphModel.IO\bin\Debug\net6.0"
//#r "ArcGraphModel.dll"
//#r "ArcGraphModel.IO.dll"

open FsSpreadsheet
open ArcGraphModel
open ArcGraphModel.IO
open FSharpAux
open FsSpreadsheet.ExcelIO
open CvTokenHelperFunctions
open OntologyHelperFunctions
open System.IO

//fsi.AddPrinter (fun (cvp : CvParam) -> 
//    cvp.ToString()
//)
//fsi.AddPrinter (fun (cvp : CvContainer) -> 
//    cvp.ToString()
//)

// ~~~~~~~~~~~~~
// INTERNALUTILS
// ~~~~~~~~~~~~~

module String =

    /// Checks if a given string is null, empty, or consisting solely of whitespaces.
    let isNullOrWhiteSpace (str : string) =
        System.String.IsNullOrWhiteSpace str

    /// Checks if an input string option is None or, if it is Some, null, empty or consisting solely of whitespaces.
    let isNoneOrWhiteSpace str =
        Option.map isNullOrWhiteSpace str
        |> Option.defaultValue true

    /// Checks if a string is a filepath.
    let isFilepath str =
        (String.contains "/" str || String.contains "\\" str) &&
        System.IO.Path.GetExtension str <> ""

    /// Splits an file address string into a triple in the form of `sheetName * rowNumber * columnNumber`.
    let splitAddress str =
        let sheetName, res = String.split '!' str |> fun arr -> arr[0], arr[1]
        let adr = FsAddress res
        sheetName, adr.RowNumber, adr.ColumnNumber

// ~~~~~~~~~~~~~

fsi.AddPrinter (fun (cvp : ICvBase) -> 
    match cvp with
    | :? UserParam as cvp -> $"UserParam [{CvBase.getCvName cvp}]" 
    | :? CvParam as cvp -> $"CvParam [{CvBase.getCvName cvp}]" 
    | :? CvContainer as cvp -> $"CvContainer [{CvBase.getCvName cvp}]" 
    | _ -> $"ICvBase [{CvBase.getCvName cvp}]"    
)


let arcPath =
    match System.Environment.MachineName with
    | "DT-P-2021-12-OM" -> @"C:\Users\revil\OneDrive\CSB-Stuff\NFDI\testARC30\"
    | "NB-W-2020-11-OM" -> @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\testARC30\"
    | _ -> @"C:\Users\HLWei\Downloads\testArc\"


// GET & TOKENIZE STUDY/ASSAY

let assPath = Path.Combine(arcPath, "assays", "aid", "isa.assay.xlsx")
let ass = FsWorkbook.fromXlsxFile assPath

let assWorksheets =
    let wss = 
        FsWorkbook.getWorksheets ass
        |> List.filter (fun ws -> ws.Name <> "Assay")
    wss |> List.iter (fun ws -> ws.RescanRows())
    wss

//let assWorksheetsWithAnnoTables = 
//    assWorksheets
//    |> List.map (
//        fun ws -> ws, ws.Tables |> List.find (fun t -> String.contains "annotationTable" t.Name )
//    )

let assPathCvP = CvParam(Terms.filepath, ParamValue.Value assPath)

//let assWs1, assAt1 = assWorksheetsWithAnnoTables.Head
let assWs1 = assWorksheets.Head

let assTokens =
    //let atRange = assAt1.RangeAddress
    //let oldFsCs = assWs1.CellCollection.GetCells(atRange.FirstAddress, atRange.LastAddress)
    //let newFcc = FsCellsCollection()
    //newFcc.Add oldFsCs
    //let newWs = FsWorksheet("dummy", [], [], newFcc)
    //newWs.RescanRows()
    //Worksheet.parseColumns newWs
    Worksheet.parseColumns assWs1

let dataFiles =
    assTokens
    |> List.filter (fun cvb -> cvb.Name = "Data File Name")


// GET & TOKENIZE INVESTIGATION

let invPath = Path.Combine(arcPath, "isa.investigation.xlsx")
let invWb = FsWorkbook.fromXlsxFile invPath

let invWorksheet = 
    let ws = invWb.GetWorksheets().Head
    ws.RescanRows()
    ws

let invPathCvP = CvParam(Terms.filepath, ParamValue.Value invPath)

let invTokens = 
    let it = Worksheet.parseRows invWorksheet
    List.iter (fun cvb -> CvAttributeCollection.tryAddAttribute invPathCvP cvb |> ignore) it
    it

invTokens[5] :?> CvContainer |> fun x -> x.Attributes

let invContainers = TokenAggregation.aggregateTokens invTokens

invContainers
|> Seq.choose CvContainer.tryCvContainer
|> Seq.filter (fun cv -> CvBase.equalsTerm Terms.assay cv )
|> Seq.head
|> CvContainer.getSingleParam "File Name"
|> Param.getValue


// CONTACTS

let invWb = FsWorkbook.fromXlsxFile invPath
let invWorksheet = 
    let ws = 
        invWb.GetWorksheets()
        |> List.find (fun ws -> ws.Name = "isa_investigation")
    ws.RescanRows()
    ws

let invPathCvP = CvParam(Terms.filepath, ParamValue.Value invPath)

let invTokens = 
    let it = Worksheet.parseRows invWorksheet
    List.iter (fun cvb -> CvAttributeCollection.tryAddAttribute invPathCvP cvb |> ignore) it
    it

let invContainers = 
    invTokens
    |> TokenAggregation.aggregateTokens 

let invStudies =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.study cv)

let invContactsContainer =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.person cv && CvContainer.isPartOfInvestigation cv)

let person3 = Seq.item 2 invContactsContainer
Validate.CvBase.person person3
let firstName = CvContainer.tryGetPropertyStringValue "given name" person3
let lastName = CvContainer.tryGetPropertyStringValue "family name" person3
match String.isNoneOrWhiteSpace firstName, String.isNoneOrWhiteSpace lastName with
| false, false -> Success
| _ -> Error (ErrorMessage.FilesystemEntry.createFromCvParam person3)
ErrorMessage.FilesystemEntry.createFromCvParam person3
ErrorMessage.Textfile.createFromCvParam person3
person3 |> fun cvParam -> CvParam.tryGetAttribute (CvTerm.getName Address.row) cvParam |> Option.get |> Param.getValueAsInt
person3.Attributes
let person2 = Seq.item 1 invContactsContainer
person2.Attributes


// STUDIES

module ArcPaths = let studiesPath = Path.Combine(arcPath, "studies")

let invStudies =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.study cv)
    |> List.ofSeq

let x : CvParam list = invStudies.Head.Properties["identifier"] |> Seq.head |> CvParam.tryCvParam |> Option.get |> CvParam.getAttributes |> List.ofSeq
invStudies.Head.Properties.Item "identifier" |> Seq.head |> Param.tryParam |> Option.get |> Param.getValueAsString


let invStudiesPaths =
    invStudies
    |> List.ofSeq
    |> List.map (
        CvContainer.tryGetSingleAs<IParam> "File Name" 
        >> Option.map (
            Param.getValueAsString 
            >> fun s -> Path.Combine(ArcPaths.studiesPath,s)
        )
    )

let invStudiesIds =
    invStudies
    |> Seq.map (
        CvContainer.tryGetSingleAs<IParam> "identifier"
        >> Option.map Param.getValueAsString
    )

invStudiesIds |> Seq.toList

let tryGetPropertyStringValue property cvContainer =
    CvContainer.tryGetSingle property cvContainer 
    |> Option.map (Param.tryParam >> Option.map Param.getValueAsString) 
    |> Option.flatten

/// Validates a person.
let person<'T when 'T :> CvContainer> (person : 'T) =
    let firstName = CvContainer.tryGetPropertyStringValue "given name" person
    let lastName = CvContainer.tryGetPropertyStringValue "family name" person
    let message = ErrorMessage.XlsxFile.createFromCvContainer person
    match String.isNoneOrWhiteSpace firstName, String.isNoneOrWhiteSpace lastName with
    | false, false -> Success
    | _ -> Error message

let p1 = Seq.head invContacts
p1.Properties
person p1
ErrorMessage.XlsxFile.createFromCvContainer p1

let p1Attributes = p1.Attributes
p1Attributes.Head |> CvAttributeCollection.tryGetAttribute ""
ErrorMessage.FilesystemEntry.createFromCvParam p1

module ThisName =
    let myFunction () = "Function in module"

type ThisName =
    | MyCase

let result1 = ThisName.myFunction() // Accessing the function from the module using qualified access

let result2 = ThisName.MyCase // Accessing the union case

invContacts
|> Seq.map (CvBase.equalsTerm Terms.name)

invContacts
|> Seq.head
//|> CvContainer.KeyCollection
//|> CvContainer.ValueCollection
//|> fun i -> i.Properties
|> CvContainer.getSingle "family name"
|> Param.tryParam
|> Option.get
|> Param.getValue

let invStudies2 =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.study cv)

invStudies |> List.ofSeq |> List.head |> CvContainer.tryGetSingleAs<IParam> "File Name" |> Option.map Param.getValueAsString

let assay1 = 
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.assay cv )
    |> Seq.head


//match invesContacts[0] with
//| p when (Param.tryParam p).IsSome -> 
//    (Param.tryParam p).Value
//    |> Param.getValue
//    |> string
//| c when (CvContainer.tryCvContainer c).IsSome -> 
//    (CvContainer.tryCvContainer c).Value
//    |> CvContainer.getSingleAs "First Name"
//    |> CvBase.getCvName
//| _ -> failwith "no."

//invesContacts[0] |> CvContainer.tryCvContainer |> Option.get |> CvContainer.getSingleAs "First Name" |> CvBase.getCvName 
//invesContacts[0] |> CvContainer.tryCvContainer |> Option.get |> CvContainer.KeyCollection |> List.ofSeq
//invesContacts[0] |> CvContainer.tryCvContainer |> Option.get |> CvContainer.ValueCollection |> List.ofSeq
//invesContacts[0] |> CvContainer.tryCvContainer |> Option.get |> CvContainer.tryGetSingle "First Name"
//invesContacts[0] |> CvContainer.tryCvContainer |> Option.get |> CvContainer.tryGetAttribute "Investigation Person First Name"
//invesContacts[0] :?> CvContainer |> fun c -> c.Properties

//invesContacts[1] |> CvContainer.tryCvContainer |> Option.get |> CvContainer.tryGetSingle "family name" |> Option.get |> Param.tryParam |> Option.get |> ParamBase.getValue |> string
//invesContacts[0] |> CvContainer.tryCvContainer |> Option.get |> CvContainer.tryGetSingle "family name" |> Option.get |> Param.tryParam |> Option.get |> ParamBase.getValue |> string