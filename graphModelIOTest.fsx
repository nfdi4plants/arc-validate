#r "nuget: FSharpAux.Core,2.0.0"
#r "nuget: FsSpreadsheet.ExcelIO"
#r "nuget: ArcGraphModel, 0.1.0-preview.1"
#r "nuget: ArcGraphModel.IO, 0.1.0-preview.1"
#I "src/ArcValidation/bin/Debug/netstandard2.0"
#r "ArcValidation.dll"

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
open ArcValidation
open CvTokenHelperFunctions
open OntologyHelperFunctions
open System.IO
open ErrorMessage

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


type Directory with

    /// Returns the names of files (including their paths) in the specified directory if they exist. Else returns None.
    static member TryGetFiles path =
        try Directory.GetFiles path |> Some
        with :? System.ArgumentException -> None

    /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory if they
    /// exist. Else returns None.
    static member TryGetFiles(path, searchPattern) =
        try Directory.GetFiles(path, searchPattern) |> Some
        with :? System.ArgumentException -> None


module Worksheet =

    let parseColumns (worksheet : FsWorksheet) = 
        let sheetName = Address.createWorksheetParam worksheet.Name
        let annoTable = worksheet.Tables |> List.tryFind (fun t -> String.contains "annotationTable" t.Name)
        match annoTable with
        | Some t ->
            t.Columns(worksheet.CellCollection)
            |> Seq.toList
            |> List.choose (fun r -> 
                match r |> Tokenization.parseLine |> Seq.toList with
                | [] -> None
                | l -> Some l
            )
            |> List.concat
            |> List.map (fun token ->        
                CvAttributeCollection.tryAddAttribute sheetName token |> ignore
                token
            )
        | None -> []

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

let assPath = Path.Combine(arcPath, "assays", "aid2", "isa.assay.xlsx")
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
let assWs2 = assWorksheets[1]
let assWs3 = assWorksheets[2]
let assWs4 = assWorksheets[3]

let assTokens1 =    // contains
    //let atRange = assAt1.RangeAddress
    //let oldFsCs = assWs1.CellCollection.GetCells(atRange.FirstAddress, atRange.LastAddress)
    //let newFcc = FsCellsCollection()
    //newFcc.Add oldFsCs
    //let newWs = FsWorksheet("dummy", [], [], newFcc)
    //newWs.RescanRows()
    //Worksheet.parseColumns newWs
    Worksheet.parseColumns assWs1

let assTokens2 = Worksheet.parseColumns assWs2
let assTokens3 = Worksheet.parseColumns assWs3
let assTokens4 = Worksheet.parseColumns assWs4

let rawDataFiles =
    assTokens3
    |> List.filter (fun cvb -> cvb.Name = "Data")

let derivedDataFiles =
    assTokens4
    |> List.filter (fun cvb -> cvb.Name = "Data")

let protocolRefs =
    assTokens1
    |> List.filter (fun cvb -> cvb.Name = "Protocol REF")

let rawDataFilepaths =
    rawDataFiles
    |> List.map (
        Param.tryParam 
        >> Option.get 
        >> Param.getValueAsString
    )

let derivedDataFilepaths =
    derivedDataFiles
    |> List.map (
        Param.tryParam 
        >> Option.get 
        >> Param.getValueAsString
    )

let protocolFilepaths =
    protocolRefs
    |> List.map (
        Param.tryParam 
        >> Option.get 
        >> Param.getValueAsString
    )


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

invContainers
|> Seq.choose CvContainer.tryCvContainer
|> Seq.find (CvBase.getCvName >> (=) "Investigation")
|> fun cvc -> cvc.Properties

invTokens
|> Seq.skipWhile (CvBase.getCvName >> (<>) "Investigation")
|> Seq.takeWhile (CvBase.getCvName >> (<>) "Study")
|> Seq.filter (CvBase.getCvName >> (=) "E-mail Address")


// CONTACTS

let invStudies =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.study cv)

let invContactsContainer =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.person cv && CvContainer.isPartOfInvestigation cv)


let person<'T when 'T :> CvContainer> (personCvContainer : 'T) =
    let firstName = CvContainer.tryGetPropertyStringValue "given name" personCvContainer
    let lastName = CvContainer.tryGetPropertyStringValue "family name" personCvContainer
    let emailAddress = CvContainer.tryGetPropertyStringValue "E-mail Address" personCvContainer
    printfn $"emailAddress: {emailAddress}"
    match String.isNoneOrWhiteSpace firstName, String.isNoneOrWhiteSpace lastName with
    | false, false -> Success
    //| _ -> Error (ErrorMessage.XlsxFile.createFromCvParam personCvContainer)
    // TO DO: Rewrite this with own CvParam creation (instead of using HLW's one) which has all ErrorMessage-related information inside
    | _ -> Error (ErrorMessage.FilesystemEntry.createFromCvParam personCvContainer)

person <| Seq.head invContactsContainer

let property<'T when 'T :> CvContainer> property (personCvContainer : 'T) =
    let personProperty = CvContainer.tryGetPropertyStringValue property personCvContainer
    if String.isNoneOrWhiteSpace personProperty then
        // TO DO: Rewrite this with own CvParam creation (instead of using HLW's one) which has all ErrorMessage-related information inside
        // Error (ErrorMessage.XlsxFile.createFromCvParam personCvContainer)
        Error (ErrorMessage.FilesystemEntry.createFromCvParam personCvContainer)
    else Success

let emailAddress<'T when 'T :> CvContainer> (personCvContainer : 'T) =
    property "E-mail Address" personCvContainer

let firstName<'T when 'T :> CvContainer> (personCvContainer : 'T) =
    property "given name" personCvContainer

let lastName<'T when 'T :> CvContainer> (personCvContainer : 'T) =
    property "family name" personCvContainer

let p1 = Seq.head invContactsContainer
p1.Properties

emailAddress p1
firstName p1
lastName p1

let p2 = Seq.item 1 invContactsContainer
p2.Properties

emailAddress p2
firstName p2
lastName p2


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
person2.Properties
let person1 = Seq.head invContactsContainer
person1.Attributes
person1.Properties

Validate.CvBase.Person.affiliation person1
Validate.CvBase.Person.affiliation person2


// STUDIES

module ArcPaths = let studiesPath = Path.Combine(arcPath, "studies")

//let x : CvParam list = invStudies.Head.Properties["identifier"] |> Seq.head |> CvParam.tryCvParam |> Option.get |> CvParam.getAttributes |> List.ofSeq
//invStudies.Head.Properties.Item "identifier" |> Seq.head |> Param.tryParam |> Option.get |> Param.getValueAsString

let invStudiesPathsAndIds =
    invStudies
    |> Seq.map (
        fun cvc ->
            CvContainer.tryGetSingleAs<IParam> "File Name" cvc
            |> Option.map (
                Param.getValueAsString 
                >> fun s -> 
                    let sLinux = String.replace "\\" "/" s
                    Path.Combine(ArcPaths.studiesPath, sLinux)
            ),
            CvContainer.tryGetSingleAs<IParam> "identifier" cvc
            |> Option.map Param.getValueAsString
    )

let foundStudyFolders = 
    Directory.GetDirectories ArcPaths.studiesPath

let foundStudyFilesAndIds = 
    foundStudyFolders
    |> Array.map (
        fun sp ->
            Directory.TryGetFiles(sp, "isa.study.xlsx") 
            |> Option.bind Array.tryHead,
            String.rev sp
            |> String.replace "\\" "/"
            |> String.takeWhile ((<>) '/')
            |> String.rev
    )

/// Validates a Study file's registration in the Investigation.
let registrationInInvestigation (investigationStudiesPathsAndIds : seq<string option * string option>) studyFilepath =
    let studyFilepathLinux = String.replace "/" "\\" studyFilepath
    let cond = 
        investigationStudiesPathsAndIds
        |> Seq.exists (
            fun (p,id) -> 
                let pLinux = Option.map (String.replace "/" "\\") p
                pLinux = Some studyFilepathLinux
        ) 
    if cond then Success
    else Error (Message.create studyFilepath None None None MessageKind.FilesystemEntryKind)

registrationInInvestigation invStudiesPathsAndIds (foundStudyFilesAndIds |> Array.head |> fst |> Option.get)

let foundStudyAnnoTables = 
    foundStudyFilesAndIds
    |> Array.choose fst
    |> Array.map (
        fun sp ->
            let std = try FsWorkbook.fromXlsxFile sp |> fun r -> printfn "try worked"; r with :? IOException -> printfn "try failed"; new FsWorkbook()
            let stdWorksheets = 
                let wss = 
                    FsWorkbook.getWorksheets std
                    |> List.filter (fun ws -> ws.Name <> "Study")
                wss |> List.iter (fun ws -> ws.RescanRows())
                wss
            let stdPathCvP = CvParam(Terms.filepath, ParamValue.Value sp)
            stdWorksheets 
            |> List.map (
                Worksheet.parseColumns
                >> List.map (
                    fun cvb ->
                        CvAttributeCollection.tryAddAttribute stdPathCvP cvb |> ignore
                        cvb
                )
            )
    )

let studyRawOrDerivedDataPaths =
    foundStudyAnnoTables
    |> Seq.collect (      // single study
        Seq.collect (      // single annoTable
            Seq.filter (
                CvBase.getCvName >> (=) "Data"
            )
        )
    )

studyRawOrDerivedDataPaths
|> Seq.map (
    Param.tryParam >> Option.get >> Param.getValueAsString
)

let studyProtocolRefPaths =
    foundStudyAnnoTables
    |> Seq.collect (      // single study
        Seq.collect (      // single annoTable
            Seq.filter (
                CvBase.getCvName >> (=) "Protocol REF"
            )
        )
    )

studyProtocolRefPaths
|> Seq.map (
    Param.tryParam >> Option.get >> Param.getValueAsString
)

let filepathParam = Seq.head studyRawOrDerivedDataPaths |> Param.tryParam |> Option.get
CvParam.tryGetAttribute "Pathname" filepathParam
(filepathParam :?> CvParam).GetAttribute "Pathname" |> Param.getValueAsString
let relFilepath = Param.getValueAsString filepathParam
let fullpath =
    // if relative path from ARC root is provided
    if String.contains  "/" relFilepath || String.contains "\\" relFilepath then
        Path.Combine(ArcPaths.inputPath, relFilepath)
    // if only filename is provided, storage in dataset folder is assumed
    else
        let sfp = (filepathParam :?> CvParam).GetAttribute "Pathname" |> Param.getValueAsString |> String.replace "/" "\\"
        let sfpTrunc =
            let i = String.findIndexBack '\\' sfp
            sfp[.. i]
        Path.Combine(sfpTrunc, "dataset", relFilepath)
if File.Exists fullpath then Success
else Error (XlsxFile.createFromCvParam filepathParam)

/// Validates a filepath.
let filepath<'T when 'T :> IParam> (filepathParam : 'T) =
    let relFilepath = Param.getValueAsString filepathParam
    let fullpath =
        // if relative path from ARC root is provided
        if String.contains  "/" relFilepath || String.contains "\\" relFilepath then
            Path.Combine(ArcPaths.inputPath, relFilepath)
        // if only filename is provided, storage in dataset folder is assumed
        else
            let elementFullpath = 
                CvParam.tryGetAttribute "Pathname" filepathParam 
                |> Option.get 
                |> Param.getValueAsString 
                |> String.replace "/" "\\"
            let fileKind = filepathParam |> CvBase.getCvName
            let subFolder =
                match fileKind with
                | "Protocol REF" -> "protocols"
                | "Data" ->
                    if elementFullpath |> String.contains "\\assays\\" then "dataset"
                    elif elementFullpath |> String.contains "\\studies\\" then "resources"
                    else ""
                | _ -> ""
            let efpTrunc =
                let i = String.findIndexBack '\\' elementFullpath
                elementFullpath[.. i]
            Path.Combine(efpTrunc, subFolder, relFilepath)
    if File.Exists fullpath then Success
    else Error (ErrorMessage.XlsxFile.createFromCvParam filepathParam)

filepath filepathParam

let filepathParam2 = Seq.head studyProtocolRefPaths |> Param.tryParam |> Option.get
filepath filepathParam2


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

let invStudies2 =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.study cv)

invStudies |> List.ofSeq |> List.head |> CvContainer.tryGetSingleAs<IParam> "File Name" |> Option.map Param.getValueAsString


// ASSAYS

let assay1 = 
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.assay cv )
    |> Seq.head

let foundAssayFolders = 
    Directory.GetDirectories ArcPaths.assaysPath

let foundAssayFilesAndIds = 
    foundAssayFolders
    |> Array.map (
        fun sp ->
            Directory.TryGetFiles(sp, "isa.assay.xlsx") 
            |> Option.bind Array.tryHead,
            String.rev sp
            |> String.replace "\\" "/"
            |> String.takeWhile ((<>) '/')
            |> String.rev
    )

let foundAssayAnnoTables = 
    foundAssayFilesAndIds
    |> Array.choose fst
    |> Array.map (
        fun sp ->
            let std = try FsWorkbook.fromXlsxFile sp |> fun r -> printfn "try worked"; r with :? IOException -> printfn "try failed"; new FsWorkbook()
            let stdWorksheets = 
                let wss = 
                    FsWorkbook.getWorksheets std
                    |> List.filter (fun ws -> ws.Name <> "Study")
                wss |> List.iter (fun ws -> ws.RescanRows())
                wss
            let stdPathCvP = CvParam(Terms.filepath, ParamValue.Value sp)
            stdWorksheets 
            |> List.map (
                Worksheet.parseColumns
                >> List.map (
                    fun cvb ->
                        CvAttributeCollection.tryAddAttribute stdPathCvP cvb |> ignore
                        cvb
                )
            )
    )

let dataPaths =
    //Seq.append foundStudyAnnoTables foundAssayAnnoTables
    foundAssayAnnoTables
    |> Seq.collect (      // single study/assay
        Seq.collect (      // single annoTable
            Seq.filter (
                CvBase.getCvName >> (fun n -> n = "Data" || n = "Protocol REF")
            )
        )
    )

dataPaths |> Seq.toList |> List.map (Param.tryParam >> Option.get (*>> Param.getValueAsString*) >> filepath)


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