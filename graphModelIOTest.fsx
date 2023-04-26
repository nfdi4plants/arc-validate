#r "nuget: FSharpAux.Core,2.0.0"
#r "nuget: FsSpreadsheet.ExcelIO"
#r "nuget: ArcGraphModel, 0.1.0-preview.1"
#r "nuget: ArcGraphModel.IO, 0.1.0-preview.1"

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

//fsi.AddPrinter (fun (cvp : CvParam) -> 
//    cvp.ToString()
//)
//fsi.AddPrinter (fun (cvp : CvContainer) -> 
//    cvp.ToString()
//)

fsi.AddPrinter (fun (cvp : ICvBase) -> 
    match cvp with
    | :? UserParam as cvp -> $"UserParam [{CvBase.getCvName cvp}]" 
    | :? CvParam as cvp -> $"CvParam [{CvBase.getCvName cvp}]" 
    | :? CvContainer as cvp -> $"CvContainer [{CvBase.getCvName cvp}]" 
    | _ -> $"ICvBase [{CvBase.getCvName cvp}]"    
)

//let p = @"C:\Users\HLWei\Downloads\testArc\isa.investigation.xlsx"
let p = @"C:\Users\revil\OneDrive\CSB-Stuff\NFDI\testARC30\isa.investigation.xlsx"
let invWb = FsWorkbook.fromXlsxFile p

let worksheet = 
    let ws = invWb.GetWorksheets().Head
    ws.RescanRows()
    ws

let tokens = 
    worksheet
    |> Worksheet.parseRows

let invContainers = 
    tokens
    |> TokenAggregation.aggregateTokens 

invContainers
|> Seq.choose CvContainer.tryCvContainer
|> Seq.filter (fun cv -> CvBase.equalsTerm Terms.assay cv )
|> Seq.head
|> CvContainer.getSingleParam "File Name"
|> Param.getValue

let inv =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.find (fun cv -> CvBase.equalsTerm Terms.person cv)

inv
|> CvBase.equalsTerm Terms.name

inv
//|> CvContainer.KeyCollection
//|> CvContainer.ValueCollection
//|> fun i -> i.Properties
|> CvContainer.getSingle "family name"
|> Param.tryParam
|> Option.get
|> Param.getValue

let invCntcts = 
    inv 
    |> CvContainer.getMany "Contacts"

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

let invesContacts = 
    invContainers
    |> List.ofSeq
    //|> List.find (fun up -> (up :> IParam).Name = "INVESTIGATION CONTACTS")
    //|> List.find (fun up -> (up :> IParam).Name = "Investigation Person First Name")
    //|> List.find (fun up -> up.Name = "Investigation Person First Name")
    |> List.find (fun c -> c.Name = "Investigation")
    |> CvContainer.tryCvContainer
    |> Option.get
    //|> List.map (fun c -> c.ToString())
    //|> fun c -> c.ToString()


let fn1 =
    CvContainer.tryGetSingle "family name" invesContacts
    |> Option.get
    |> CvParam.tryCvParam
    |> Option.get

fn1
|> Param.getValue
|> string

fn1.Attributes

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