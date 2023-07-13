namespace ArcValidation


module InformationExtraction =

    open FsSpreadsheet
    open FsSpreadsheet.ExcelIO
    open ArcGraphModel
    open ArcGraphModel.IO
    open CvTokenHelperFunctions
    open System.IO
    open FSharpAux


    module Investigation =

        /// Returns the workbook from a path to an Investigation file.
        let getWorkbook (investigationPath : string) = 
            FsWorkbook.fromXlsxFile investigationPath

        /// Returns the Investigation worksheet of an Investigation file's workbook.
        let getWorksheet (invWb : FsWorkbook) = 
            let ws = 
                invWb.GetWorksheets()
                |> List.find (fun ws -> ws.Name = "isa_investigation")
            ws.RescanRows()
            ws

        /// Returns a CvParam of a given path to an Investigation file.
        let getPathCvP (investigationPath : string) = 
            CvParam(Terms.filepath, ParamValue.Value investigationPath)

        /// Tokenizes an Investigation worksheet and returns the ICvBase list based on it.
        let getTokens (invPathCvP : CvParam) (invWorksheet : FsWorksheet) = 
            let it = Worksheet.parseRows invWorksheet
            List.iter (fun cvb -> CvAttributeCollection.tryAddAttribute invPathCvP cvb |> ignore) it
            it

        /// Aggregates ICvBases and returns CvContainers and CvParams as ICvBases.
        let getContainers (invTokens : ICvBase list) = 
            invTokens
            |> TokenAggregation.aggregateTokens 

        /// Returns all CvContainers whose CvBase terms equals the term of Investigation.
        let getInvestigationContainer (invContainers : seq<#ICvBase>) =
            invContainers
            |> Seq.choose CvContainer.tryCvContainer
            |> Seq.filter (CvBase.equalsTerm Terms.investigation)

        /// Returns all CvContainers whose CvBase terms equal the term of Study.
        let getStudies (invContainers : seq<#ICvBase>) =
            invContainers
            |> Seq.choose CvContainer.tryCvContainer
            |> Seq.filter (CvBase.equalsTerm Terms.study)

        /// Returns all CvContainers whose CvBase terms equal the term of Person and that inhabit Investigation properties.
        let getContactsContainer (invContainers : seq<#ICvBase>) =
            invContainers
            |> Seq.choose CvContainer.tryCvContainer
            |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.person cv && CvContainer.isPartOfInvestigation cv)
            //|> Seq.filter (fun cv -> CvBase.equalsTerm Terms.person cv && CvContainer.tryGetAttribute (CvTerm.getName Terms.investigation) cv |> Option.isSome)


    module Study =

        let getPathsAndIds (studiesPath : string) (invStudies : seq<#CvContainer>) =
            invStudies
            |> Seq.map (
                fun cvc ->
                    CvContainer.tryGetSingleAs<IParam> "File Name" cvc
                    |> Option.map (
                        Param.getValueAsString 
                        >> fun s -> 
                            let sLinux = String.replace "\\" "/" s
                            Path.Combine(studiesPath, sLinux)
                    ),
                    CvContainer.tryGetSingleAs<IParam> "identifier" cvc
                    |> Option.map Param.getValueAsString
            )

        let getFolders (studiesPath : string) = 
            Directory.GetDirectories studiesPath

        let getFilesAndIds (foundStudyFolders : string []) = 
            foundStudyFolders
            |> Array.map (
                fun sp ->
                    Directory.TryGetFiles(sp, "isa.study.xlsx") 
                    |> Option.bind Array.tryHead,
                    String.rev sp
                    |> String.takeWhile (fun c -> c <> '\\' && c <> '/')
                    |> String.rev
            )

        let getAnnotationTables (foundStudyFilesAndIds : (string option * string) [])= 
            foundStudyFilesAndIds
            |> Array.choose fst
            |> Array.map (
                fun sp ->
                    let std = try FsWorkbook.fromXlsxFile sp with :? IOException -> new FsWorkbook()
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

        let getRawOrDerivedDataPaths (foundStudyAnnoTables : ICvBase list list array)=
            foundStudyAnnoTables
            |> Seq.collect (        // single study
                Seq.collect (       // single annoTable
                    Seq.filter (
                        CvBase.getCvName >> (fun n -> n = "Data" || n = "Protocol REF")
                    )
                )
            )


    module Assay =

        let getFolders (assaysPath : string) =  
            Directory.GetDirectories assaysPath

        let getFilesAndIds (foundAssayFolders : string []) = 
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

        let getAnnotationTables (foundAssayFilesAndIds : (string option * string) []) = 
            foundAssayFilesAndIds
            |> Array.choose fst
            |> Array.map (
                fun ap ->
                    let atd = try FsWorkbook.fromXlsxFile ap with :? IOException -> new FsWorkbook()
                    let atdWorksheets = 
                        let wss = 
                            FsWorkbook.getWorksheets atd
                            |> List.filter (fun ws -> ws.Name <> "Assay")
                        wss |> List.iter (fun ws -> ws.RescanRows())
                        wss
                    let atdPathCvP = CvParam(Terms.filepath, ParamValue.Value ap)
                    atdWorksheets 
                    |> List.map (
                        Worksheet.parseColumns
                        >> List.map (
                            fun cvb ->
                                CvAttributeCollection.tryAddAttribute atdPathCvP cvb |> ignore
                                cvb
                        )
                    )
            )

        let getRawOrDerivedDataPaths (foundAssayAnnoTables : ICvBase list list array)=
            foundAssayAnnoTables
            |> Seq.collect (        // single assay
                Seq.collect (       // single annoTable
                    Seq.filter (
                        CvBase.getCvName >> (fun n -> n = "Data" || n = "Protocol REF")
                    )
                )
            )