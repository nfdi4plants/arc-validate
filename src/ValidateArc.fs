module ValidateArc

open System.IO
open OntologyHelperFunctions
open CheckFilesystemStructure.Paths
open CheckFilesystemStructure.Checks
open CheckIsaStructure
open Expecto

let filesystem =
    testList "Filesystem" [
        testCase ".arc" <| fun () -> isPresent hasArcFolder (createMessage arcFolderPath None None None FilesystemEntry)
        testList ".git" [
            testCase ".git folder"      <| fun () -> isPresent hasGitFolder     (createMessage gitFolderPath    None None None FilesystemEntry)
            testCase "hooks folder"     <| fun () -> isPresent hasHooksFolder   (createMessage hooksPath        None None None FilesystemEntry)
            testCase "objects folder"   <| fun () -> isPresent hasObjectsFolder (createMessage objectsPath      None None None FilesystemEntry)
            testCase "refs folder"      <| fun () -> isPresent hasRefsFolder    (createMessage refsPath         None None None FilesystemEntry)
        ]
    ]

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


let isaTests =
    testList "ISA" [
        testList "Schema" [
            testList "Study" [
                // TO DO: By god, make this abomination of uglyness somehow pretty!
                yield!
                    match allStudies with
                    | Some multipleS -> 
                        multipleS 
                        |> Seq.choose (
                            fun s ->
                                if s.HasIsaFile then
                                    let sPath = Path.Combine(s.Path, "isa.study.xlsx")
                                    let hasStudySourceNameColumn = Study.isSourceNameColumnPresent sPath 
                                    testList "Worksheet" [
                                        yield!
                                            hasStudySourceNameColumn
                                            |> Seq.choose (
                                                fun (hsnc, sheetName, line, pos) -> 
                                                    if sheetName = "" then None
                                                    else 
                                                        testCase 
                                                            "SourceNameColumn"
                                                            (fun () -> isPresent hsnc (createMessage sPath (Some line) (Some pos) (Some sheetName) XLSXFile))
                                                        |> Some
                                            )
                                    ]
                                    |> Some
                                else None
                        )
                    | None -> Seq.empty
            ]
        ]
        testList "Semantic" [
            testList "Study" [
                
            ]
        ]
        testList "Plausibility" [
            testList "Study" [
                
            ]
        ]
    ]