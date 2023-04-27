module ValidateArc

open OntologyHelperFunctions
open ArcPaths
open CheckIsaStructure
open Expecto
open ArcGraphModel
open ArcGraphModel.IO
open FsSpreadsheet
open FsSpreadsheet.ExcelIO
open ErrorMessage.FailStrings
open System.IO
open FSharpAux
open CvTokens


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

let invContacts =
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
                >> fun s -> Path.Combine(ArcPaths.studiesPath,s)
            ),
            CvContainer.tryGetSingleAs<IParam> "identifier" cvc
            |> Option.map Param.getValueAsString
    )


[<Tests>]
let filesystem =
    testList "Filesystem" [
        testCase "arcFolder"            <| fun () -> Validate.FilesystemEntry.folder dotArcFolderPath   |> throwError FilesystemEntry.isPresent
        testCase "InvestigationFile"    <| fun () -> Validate.FilesystemEntry.file investigationPath    |> throwError FilesystemEntry.isPresent
        testCase "StudiesFolder"        <| fun () -> Validate.FilesystemEntry.folder studiesPath        |> throwError FilesystemEntry.isPresent
        testCase "AssaysFolder"         <| fun () -> Validate.FilesystemEntry.folder assaysPath         |> throwError FilesystemEntry.isPresent
        testList "Git" [
            testCase "gitFolder"            <| fun () -> Validate.FilesystemEntry.folder gitFolderPath      |> throwError FilesystemEntry.isPresent
            testCase "hooksFolder"          <| fun () -> Validate.FilesystemEntry.folder hooksPath          |> throwError FilesystemEntry.isPresent
            testCase "applyPatchFile"       <| fun () -> Validate.FilesystemEntry.file applyPatchPath       |> throwError FilesystemEntry.isPresent
            testCase "commitSampleFile"     <| fun () -> Validate.FilesystemEntry.file commitSamplePath     |> throwError FilesystemEntry.isPresent
            testCase "fsmonitorFile"        <| fun () -> Validate.FilesystemEntry.file fsmonitorPath        |> throwError FilesystemEntry.isPresent
            testCase "postUpdateFile"       <| fun () -> Validate.FilesystemEntry.file postUpdatePath       |> throwError FilesystemEntry.isPresent
            testCase "preApplyPatchFile"    <| fun () -> Validate.FilesystemEntry.file preApplyPatchPath    |> throwError FilesystemEntry.isPresent
            testCase "preCommitFile"        <| fun () -> Validate.FilesystemEntry.file preCommitPath        |> throwError FilesystemEntry.isPresent
            testCase "preMergeCommitFile"   <| fun () -> Validate.FilesystemEntry.file preMergeCommitPath   |> throwError FilesystemEntry.isPresent
            testCase "prePushFile"          <| fun () -> Validate.FilesystemEntry.file prePushPath          |> throwError FilesystemEntry.isPresent
            testCase "preRebaseFile"        <| fun () -> Validate.FilesystemEntry.file preRebasePath        |> throwError FilesystemEntry.isPresent
            testCase "preReceiveFile"       <| fun () -> Validate.FilesystemEntry.file preReceivePath       |> throwError FilesystemEntry.isPresent
            testCase "prepareCommitFile"    <| fun () -> Validate.FilesystemEntry.file prepareCommitPath    |> throwError FilesystemEntry.isPresent
            testCase "pushToCheckoutFile"   <| fun () -> Validate.FilesystemEntry.file pushToCheckoutPath   |> throwError FilesystemEntry.isPresent
            testCase "updateFile"           <| fun () -> Validate.FilesystemEntry.file updatePath           |> throwError FilesystemEntry.isPresent
            testCase "infoFolder"           <| fun () -> Validate.FilesystemEntry.folder infoPath           |> throwError FilesystemEntry.isPresent
            testCase "excludeFile"          <| fun () -> Validate.FilesystemEntry.file excludePath          |> throwError FilesystemEntry.isPresent
            testCase "objectsPackFolder"    <| fun () -> Validate.FilesystemEntry.folder objectsPackPath    |> throwError FilesystemEntry.isPresent
            testCase "refsFolder"           <| fun () -> Validate.FilesystemEntry.folder refsPath           |> throwError FilesystemEntry.isPresent
            testCase "refsHeadsFolder"      <| fun () -> Validate.FilesystemEntry.folder refsHeadsPath      |> throwError FilesystemEntry.isPresent
            testCase "refsTagsFolder"       <| fun () -> Validate.FilesystemEntry.folder refsTagsPath       |> throwError FilesystemEntry.isPresent
        ]
        //testList "Studies" [
        //    for (p,id) in invStudiesPathsAndIds do
        //        if p.IsSome then
        //            let defId = Option.defaultValue "(no Study identifier)" id
        //            testCase $"{defId}" <| fun () -> Validate.FilesystemEntry.studyFolder p.Value |> throwError FilesystemEntry.isPresent
        //]
        testList "DataPathNames" [
            
        ]
    ]


[<Tests>]
let isaTests =
    testList "ISA" [
        testList "Semantic" [
            testList "Investigation" [
                testList "Person" (
                    invContacts
                    |> List.ofSeq
                    |> List.mapi (
                        fun i p ->
                            printfn $"{i}, {p}"
                            testCase $"Person{i + 1}" (fun () -> Validate.CvBase.person p |> throwError XLSXFile.isValidTerm)
                    )
                )
            ]
        ]
    ]