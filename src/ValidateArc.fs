module ValidateArc

open OntologyHelperFunctions
open ArcPaths
open CheckIsaStructure
open Expecto
open ArcGraphModel
open ArcGraphModel.IO
open ErrorMessage.FailStrings
open FSharpAux
open InformationExtraction
open System.IO


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
        testList "Studies" [
            for (p,id) in invStudiesPathsAndIds do
                if p.IsSome then
                    let defId = Option.defaultValue "(no Study identifier)" id
                    testCase $"{defId}" <| fun () -> Validate.FilesystemEntry.file p.Value |> throwError FilesystemEntry.isPresent
        ]
        testList "DataPathNames" [
            
        ]
    ]


[<Tests>]
let isaTests =
    testList "ISA" [
        testList "Semantic" [
            testList "Investigation" [
                testCase "Contacts" <| fun () -> Validate.CvBase.contacts "" invContactsContainer |> throwError FilesystemEntry.isPresent
                testList "Person" (
                    invContactsContainer
                    |> List.ofSeq
                    |> List.mapi (
                        fun i p ->
                            // commented out until CvParam filling is done
                            //testCase $"Person{i + 1}" <| fun () -> Validate.CvBase.person p |> throwError XLSXFile.isValidTerm
                            testCase $"Person{i + 1}" <| fun () -> Validate.CvBase.person p |> throwError FilesystemEntry.isValidTerm
                    )
                )
                testList "Studies" [
                    for (p,id) in invStudiesPathsAndIds do
                        if p.IsNone && id.IsSome then
                            let assumedFilename = Path.Combine(ArcPaths.studiesPath, $"{id.Value}\\isa.study.xlsx")
                            let errorMessage = ErrorMessage.FilesystemEntry.createFromFile assumedFilename |> Error
                            testCase $"{id.Value}" <| fun () -> throwError FilesystemEntry.isPresent errorMessage
                        if p.IsNone && id.IsNone then
                            // commented out until CvParam filling is done
                            //testCase $"Person{i + 1}" <| fun () -> Validate.CvBase.person p |> throwError XLSXFile.isValidTerm
                            //testCase "(no Study identifier)" <| fun () -> throwError XLSXFile.isRegistered investigationPath
                            let errorMessage = ErrorMessage.FilesystemEntry.createFromFile investigationPath |> Error
                            testCase "(no Study identifier)" <| fun () -> throwError FilesystemEntry.isRegistered errorMessage
                ]
            ]
        ]
    ]