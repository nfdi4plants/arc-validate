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
open CvTokens
open OntologyHelperFunctions

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

//let p = @"C:\Users\HLWei\Downloads\testArc\isa.investigation.xlsx"
let investigationPath = @"C:\Users\revil\OneDrive\CSB-Stuff\NFDI\testARC30\isa.investigation.xlsx"
let invWb = FsWorkbook.fromXlsxFile investigationPath

let invWorksheet = 
    let ws = invWb.GetWorksheets().Head
    ws.RescanRows()
    ws

let invPathCvP = CvParam(Terms.filepath, ParamValue.Value investigationPath)

let invTokens = 
    let it = Worksheet.parseRows invWorksheet
    List.iter (fun cvb -> CvAttributeCollection.tryAddAttribute invPathCvP cvb |> ignore) it
    it

let invContainers = TokenAggregation.aggregateTokens invTokens

invContainers
|> Seq.choose CvContainer.tryCvContainer
|> Seq.filter (fun cv -> CvBase.equalsTerm Terms.assay cv )
|> Seq.head
|> CvContainer.getSingleParam "File Name"
|> Param.getValue

let omgCondition (cvc : CvContainer) =
    cvc.Properties.Values
    |> Seq.exists (
        Seq.exists (
            CvParam.tryCvParam
            >> Option.get
            >> fun cvp -> cvp.Attributes
            >> List.exists (
                fun ip -> CvBase.getCvName ip = CvTerm.getName Terms.investigation
            )
        )
    )

let invContacts =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.person cv && omgCondition cv)

invContacts |> Seq.head |> fun c -> c.Attributes
invContacts |> Seq.toList |> List.map (fun c -> c.Attributes)

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

let invStudies =
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