module InformationExtraction

open FsSpreadsheet
open FsSpreadsheet.ExcelIO
open ArcPaths
open ArcGraphModel
open ArcGraphModel.IO
open CvTokenHelperFunctions
open System.IO
open FSharpAux


let invWb = FsWorkbook.fromXlsxFile investigationPath
let invWorksheet = 
    let ws = 
        invWb.GetWorksheets()
        |> List.find (fun ws -> ws.Name = "isa_investigation")
    ws.RescanRows()
    ws

let invPathCvP = CvParam(Terms.filepath, ParamValue.Value investigationPath)

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
    //|> Seq.filter (fun cv -> CvBase.equalsTerm Terms.person cv && CvContainer.tryGetAttribute (CvTerm.getName Terms.investigation) cv |> Option.isSome)

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
            |> String.takeWhile (fun c -> c <> '\\' && c <> '/')
            |> String.rev
    )

let foundStudyAnnoTables = 
    foundStudyFilesAndIds
    |> Array.choose fst
    |> Array.map (
        fun sp ->
            printfn "%A" sp
            let std = try FsWorkbook.fromXlsxFile sp with _ -> new FsWorkbook()
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
    |> Seq.collect (        // single study
        Seq.collect (       // single annoTable
            Seq.filter (
                CvBase.getCvName >> (=) "Data"
            )
        )
    )