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
    let res1 = 
        invContainers
        |> Seq.choose CvContainer.tryCvContainer
    let res2 =
        res1
        |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.person cv && omgCondition cv)
    res2

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
        testCase "arcFolder"            <| fun () -> Validate.FilesystemEntry.dotArcFolder dotArcFolderPath         |> throwError FilesystemEntry.isPresent
        testCase "InvestigationFile"    <| fun () -> Validate.FilesystemEntry.investigationFile investigationPath   |> throwError FilesystemEntry.isPresent
        testCase "StudiesFolder"        <| fun () -> Validate.FilesystemEntry.studiesFolder studiesPath             |> throwError FilesystemEntry.isPresent
        testCase "AssaysFolder"         <| fun () -> Validate.FilesystemEntry.assaysFolder assaysPath               |> throwError FilesystemEntry.isPresent
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