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



let invWb = FsWorkbook.fromXlsxFile investigationPath
let invWorksheet = 
    let ws = 
        invWb.GetWorksheets()
        |> List.find (fun ws -> ws.Name = "isa_investigation")
    ws.RescanRows()
    ws

let invTokens = 
    invWorksheet
    |> Worksheet.parseRows

let invContainers = 
    invTokens
    |> TokenAggregation.aggregateTokens 

let invStudies =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.study cv)

//let invStudiesPaths =
//    invStudies
//    |> Seq.map (CvContainer.tryGetSingleAs<IParam> "File Name" >> Option.map Param.getValueAsString)
//    |> Option.map 


[<Tests>]
let filesystem =
    testList "Filesystem" [
        testCase "arcFolder" <| fun () -> Validate.FilesystemEntry.dotArcFolder dotArcFolderPath |> throwError FilesystemEntry.isPresent
        testCase "StudiesFolder" <| fun () -> Validate.FilesystemEntry.studiesFolder studiesPath |> throwError FilesystemEntry.isPresent
        testCase "AssaysFolder" <| fun () -> Validate.FilesystemEntry.assaysFolder assaysPath |> throwError FilesystemEntry.isPresent
        testList "DataPathNames" [
        ]
    ]


let invContacts =
    invContainers
    |> Seq.choose CvContainer.tryCvContainer
    |> Seq.filter (fun cv -> CvBase.equalsTerm Terms.person cv)


[<Tests>]
let isaTests =
    testList "ISA" [
        testList "Semantic" [
            testList "Investigation" [
                testCase "Person" <| fun () -> 
                    Validate.CvBase.persons invContacts |> Seq.iter (throwError XLSXFile.isRegistered)
            ]
        ]
    ]