module ValidateArc

open System.IO
open OntologyHelperFunctions
open FilesystemStructure.Paths
open CheckIsaStructure
open Expecto
open ArcGraphModel
open FsSpreadsheet
open FsSpreadsheet.ExcelIO
open ValidateCvBase
open FailStrings



[<Tests>]
let filesystem =
    testList "Filesystem" [
        //testCase "arcFolder" <| fun () -> isPresent hasArcFolder (createMessage arcFolderPath None None None FilesystemEntry)
        //testList "Git" [
        //    testCase "git folder"      <| fun () -> isPresent hasGitFolder     (createMessage gitFolderPath    None None None FilesystemEntry)
        //    testCase "hooks folder"     <| fun () -> isPresent hasHooksFolder   (createMessage hooksPath        None None None FilesystemEntry)
        //    testCase "objects folder"   <| fun () -> isPresent hasObjectsFolder (createMessage objectsPath      None None None FilesystemEntry)
        //    testCase "refs folder"      <| fun () -> isPresent hasRefsFolder    (createMessage refsPath         None None None FilesystemEntry)
        //]
        testList "DataPathNames" [
            //testCase "ProtocolREFs" <| fun () -> protocolRefFilepaths |> isPresent
            //testCase "Data" <| fun () -> protocolRefFilepaths |> isPresent
            //testCase "StudyFileName" <| fun () -> protocolRefFilepaths |> isPresent
            //testCase "AssayFileName" <| fun () -> protocolRefFilepaths |> isPresent
        ]
    ]


let studies = None

let lukasFunktion = fun str -> [CvParam("","","",ParamValue.Value "")]      // dummy
let inv = FsWorkbook.fromXlsxFile investigationPath
let worksheet = 
    let ws = 
        inv.GetWorksheets()
        |> List.find (fun ws -> ws.Name = "isa_investigation")
    ws.RescanRows()
    ws
let investigationPersons : #ICvBase list = lukasFunktion investigationPath

let tokens = 
    worksheet
    |> Worksheet.parseRows

let containers = 
    tokens
    |> TokenAggregation.aggregateTokens 

[<Tests>]
let isaTests =
    testList "ISA" [
        //testList "Schema" [
        //    testList "Study" [
        //        match studies with
        //        | None -> ()
        //        | Some ss ->
        //            ss
        //            |> List.iter (
        //                fun s ->
                            
        //            )
        //    ]
        //]
        testList "Semantic" [
            testList "Investigation" [
                testCase "Person" <| fun () -> 
                    ValidateCvBase.persons investigationPersons |> List.iter (throwError XLSXFile.isRegistered)
            ]
        ]
    ]

//[<Tests>]
//let isaTests =
//    testList "ISA" [
//        testList "Schema" [
//            testList "Study" [
//                // TO DO: By god, make this abomination of uglyness somehow pretty!
//                yield!
//                    match allStudies with
//                    | Some multipleS -> 
//                        multipleS 
//                        |> Seq.choose (
//                            fun s ->
//                                if s.HasIsaFile then
//                                    let sPath = Path.Combine(s.Path, "isa.study.xlsx")
//                                    let hasStudySourceNameColumn = Study.isSourceNameColumnPresent sPath 
//                                    testList "Worksheet" [
//                                        yield!
//                                            hasStudySourceNameColumn
//                                            |> Seq.choose (
//                                                fun (hsnc, sheetName, line, pos) -> 
//                                                    if sheetName = "" then None
//                                                    else 
//                                                        testCase 
//                                                            "SourceNameColumn"
//                                                            (fun () -> isPresent hsnc (createMessage sPath (Some line) (Some pos) (Some sheetName) XLSXFile))
//                                                        |> Some
//                                            )
//                                    ]
//                                    |> Some
//                                else None
//                        )
//                    | None -> Seq.empty
//            ]
//        //]
        //testList "Semantic" [
        //    testList "Investigation" [
        //        testCase "Person" <| fun () -> Validate.persons investigationPersons |> List.iter (throwError FailStrings.XLSXFile.isPresent)
        //    ]
        //]
//        //testList "Plausibility" [
//        //    testList "Study" [
                
//        //    ]
//        ]
    //]

//let isaTests =
//    isa [
//        schema [
//            Sublevel2.study [
//                sourceNameColumn (fun () -> isPresent studySourceNameColumn (XlsxFileMessage studyXlsx))  // !DONE!
//                sampleNameColumn (fun () -> isPresent studySampleNameColumn (XlsxFileMessage studyXlsx))
//            ]
//            assay (fun () -> isRegistered studyRegisteredInInves (XlsxFileMessage invesXlsx))
//        ]
//        semantic [
//            Sublevel2.assay [
//                term (fun () -> isValidTerm termsAvailable1 (XlsxFileMessage assayXlsx))
//                term (fun () -> isValidTerm termsAvailable2 (XlsxFileMessage assayXlsx))
//            ]
//        ]
//        plausibility [
//            Sublevel2.study [
//                factor (fun () -> isPresent studyFactor (XlsxFileMessage studyXlsx))
//            ]
//        ]
//    ]