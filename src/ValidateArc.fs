module ValidateArc

open OntologyHelperFunctions
open CheckIsaStructure
open Expecto
open ArcGraphModel
open ArcGraphModel.IO
open ErrorMessage.FailStrings
open FSharpAux
open InformationExtraction
open System.IO

open Defaults 

[<Tests>]
let filesystem =
    testList "Filesystem" [
        testCase "arcFolder"            <| fun () -> Validate.FilesystemEntry.folder arcPaths.DotArcFolderPath   |> throwError FilesystemEntry.isPresent
        testCase "InvestigationFile"    <| fun () -> Validate.FilesystemEntry.file arcPaths.InvestigationPath    |> throwError FilesystemEntry.isPresent
        testCase "StudiesFolder"        <| fun () -> Validate.FilesystemEntry.folder arcPaths.StudiesPath        |> throwError FilesystemEntry.isPresent
        testCase "AssaysFolder"         <| fun () -> Validate.FilesystemEntry.folder arcPaths.AssaysPath         |> throwError FilesystemEntry.isPresent
        testList "Git" [
            testCase "gitFolder"            <| fun () -> Validate.FilesystemEntry.folder arcPaths.GitFolderPath      |> throwError FilesystemEntry.isPresent
            testCase "hooksFolder"          <| fun () -> Validate.FilesystemEntry.folder arcPaths.HooksPath          |> throwError FilesystemEntry.isPresent
            testCase "applyPatchFile"       <| fun () -> Validate.FilesystemEntry.file arcPaths.ApplyPatchPath       |> throwError FilesystemEntry.isPresent
            testCase "commitSampleFile"     <| fun () -> Validate.FilesystemEntry.file arcPaths.CommitSamplePath     |> throwError FilesystemEntry.isPresent
            testCase "fsmonitorFile"        <| fun () -> Validate.FilesystemEntry.file arcPaths.FsmonitorPath        |> throwError FilesystemEntry.isPresent
            testCase "postUpdateFile"       <| fun () -> Validate.FilesystemEntry.file arcPaths.PostUpdatePath       |> throwError FilesystemEntry.isPresent
            testCase "preApplyPatchFile"    <| fun () -> Validate.FilesystemEntry.file arcPaths.PreApplyPatchPath    |> throwError FilesystemEntry.isPresent
            testCase "preCommitFile"        <| fun () -> Validate.FilesystemEntry.file arcPaths.PreCommitPath        |> throwError FilesystemEntry.isPresent
            testCase "preMergeCommitFile"   <| fun () -> Validate.FilesystemEntry.file arcPaths.PreMergeCommitPath   |> throwError FilesystemEntry.isPresent
            testCase "prePushFile"          <| fun () -> Validate.FilesystemEntry.file arcPaths.PrePushPath          |> throwError FilesystemEntry.isPresent
            testCase "preRebaseFile"        <| fun () -> Validate.FilesystemEntry.file arcPaths.PreRebasePath        |> throwError FilesystemEntry.isPresent
            testCase "preReceiveFile"       <| fun () -> Validate.FilesystemEntry.file arcPaths.PreReceivePath       |> throwError FilesystemEntry.isPresent
            testCase "prepareCommitFile"    <| fun () -> Validate.FilesystemEntry.file arcPaths.PrepareCommitPath    |> throwError FilesystemEntry.isPresent
            testCase "pushToCheckoutFile"   <| fun () -> Validate.FilesystemEntry.file arcPaths.PushToCheckoutPath   |> throwError FilesystemEntry.isPresent
            testCase "updateFile"           <| fun () -> Validate.FilesystemEntry.file arcPaths.UpdatePath           |> throwError FilesystemEntry.isPresent
            testCase "infoFolder"           <| fun () -> Validate.FilesystemEntry.folder arcPaths.InfoPath           |> throwError FilesystemEntry.isPresent
            testCase "excludeFile"          <| fun () -> Validate.FilesystemEntry.file arcPaths.ExcludePath          |> throwError FilesystemEntry.isPresent
            testCase "objectsPackFolder"    <| fun () -> Validate.FilesystemEntry.folder arcPaths.ObjectsPackPath    |> throwError FilesystemEntry.isPresent
            testCase "refsFolder"           <| fun () -> Validate.FilesystemEntry.folder arcPaths.RefsPath           |> throwError FilesystemEntry.isPresent
            testCase "refsHeadsFolder"      <| fun () -> Validate.FilesystemEntry.folder arcPaths.RefsHeadsPath      |> throwError FilesystemEntry.isPresent
            testCase "refsTagsFolder"       <| fun () -> Validate.FilesystemEntry.folder arcPaths.RefsTagsPath       |> throwError FilesystemEntry.isPresent
        ]
        testList "Studies" [
            for (p,id) in invStudiesPathsAndIds do
                // Validate every present Study filepath in Investigation for presence in the ARC filesystem:
                if p.IsSome then
                    let defId = Option.defaultValue "(no Study identifier)" id
                    testCase $"{defId}" <| fun () -> Validate.FilesystemEntry.file p.Value |> throwError FilesystemEntry.isPresent
            for (p,id) in foundStudyFilesAndIds do
                // Validate every Study in the ARC filesystem that has no Study file: (outcome will always be Error)
                if p.IsNone then
                    let assumedFilepath = Path.Combine(arcPaths.StudiesPath, id, "isa.study.xlsx")
                    testCase $"{id}" <| fun () -> Validate.FilesystemEntry.file assumedFilepath |> throwError FilesystemEntry.isPresent
        ]
        testList "Assays" [
            for (p,id) in foundAssayFilesAndIds do
                // Validate every Assay in the ARC filesystem that has no Assay file: (outcome will always be Error)
                if p.IsNone then
                    let assumedFilepath = Path.Combine(ArcPaths.studiesPath, id, "isa.assay.xlsx")
                    testCase $"{id}" <| fun () -> Validate.FilesystemEntry.file assumedFilepath |> throwError FilesystemEntry.isPresent
        ]
        testList "DataPathNames" [
            for fp in dataPaths do
                let fpParam = Param.tryParam fp |> Option.get
                let fpValue = Param.getValueAsString fpParam
                testCase $"{fpValue}" <| fun () -> Validate.Param.filepath fpParam |> throwError XLSXFile.isPresent
        ]
    ]


[<Tests>]
let isaTests =
    testList "ISA" [
        testList "Semantic" [
            testList "Investigation" [
                // Validate the existence of any Person in Investigation Contacts section:
                testCase "Contacts" <| fun () -> Validate.CvBase.contacts "" invContactsContainer |> throwError FilesystemEntry.isPresent
                testList "Person" (
                    invContactsContainer
                    |> List.ofSeq
                    |> List.mapi (
                        fun i p ->
                            // Validate the sufficiency of a Person in Investigation Contacts section (a Person is sufficient when both first and last name are present):
                            testCase $"Person{i + 1}" <| fun () -> Validate.CvBase.person p |> throwError FilesystemEntry.isValidTerm
                            // commented out until CvParam filling is done
                            //testCase $"Person{i + 1}" <| fun () -> Validate.CvBase.person p |> throwError XLSXFile.isValidTerm
                    )
                )
                testList "Studies" [
                    for (p,id) in invStudiesPathsAndIds do
                        if p.IsNone && id.IsSome then
                            let assumedFilename = Path.Combine(arcPaths.StudiesPath, $"{id.Value}\\isa.study.xlsx")
                            let errorMessage = ErrorMessage.FilesystemEntry.createFromFile assumedFilename |> Error
                            // Validate every Study in the Investigation that has no Study filename: (outcome will always be Error)
                            testCase $"{id.Value}" <| fun () -> throwError FilesystemEntry.isPresent errorMessage
                        if p.IsNone && id.IsNone then
                            let errorMessage = ErrorMessage.FilesystemEntry.createFromFile arcPaths.InvestigationPath |> Error
                            // Validate every Study in the Investigation that neither has an Identifier nor a filename: (outcome will always be Error)
                            testCase "(no Study identifier)" <| fun () -> throwError FilesystemEntry.isRegistered errorMessage
                            // commented out until CvParam filling is done
                            //testCase $"Person{i + 1}" <| fun () -> Validate.CvBase.person p |> throwError XLSXFile.isValidTerm
                            //testCase "(no Study identifier)" <| fun () -> throwError XLSXFile.isRegistered investigationPath

                    for (p,id) in foundStudyFilesAndIds do
                        if p.IsSome then
                            // Validate every Study in the ARC filesystem for registration in the Investigation:
                            testCase $"{id}" <| fun () -> 
                                Validate.FilesystemEntry.StudyFile.registrationInInvestigation invStudiesPathsAndIds p.Value |> throwError FilesystemEntry.isRegistered
                ]
                testList "Assays" [
                    //for (p,id) in invAssaysPathsAndIds do

                    for (p,id) in foundAssayFilesAndIds do
                        if p.IsSome then
                            // Validate every Assay in the ARC filesystem for registration in the Investigation:
                            testCase $"{id}" <| fun () ->
                                Validate.FilesystemEntry.A
                ]
            ]
        ]
    ]