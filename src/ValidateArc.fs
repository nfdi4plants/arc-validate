module ValidateArc

open OntologyHelperFunctions
open CheckFilesystemStructure
open CheckIsaStructure
open Expecto

let filesystem =
    testList "Filesystem" [
        testCase ".arc" <| fun () -> isPresent hasArcFolder (createMessage arcFolderPath None None)
        testList ".git" [
            testCase ".git folder"      <| fun () -> isPresent hasGitFolder     (createMessage gitFolderPath    None None)
            testCase "hooks folder"     <| fun () -> isPresent hasHooksFolder   (createMessage hooksPath        None None)
            testCase "objects folder"   <| fun () -> isPresent hasObjectsFolder (createMessage objectsPath      None None)
            testCase "refs folder"      <| fun () -> isPresent hasRefsFolder    (createMessage refsPath         None None)
        ]
    ]

//let isaTests =
//    isa [
//        schema [
//            Sublevel2.study [
//                sourceNameColumn (fun () -> isPresent studySourceNameColumn (XlsxFileMessage studyXlsx))
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
                testCase "SourceNameColumn" <| fun () -> isPresent 
            ]
        ]
    ]