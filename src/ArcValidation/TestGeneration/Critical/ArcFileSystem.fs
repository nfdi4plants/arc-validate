namespace ArcValidation.TestGeneration.Critical.Arc

open ArcValidation
open ArcValidation.Configs

module FileSystem =

    open OntologyHelperFunctions
    open Expecto
    open ArcGraphModel
    open ErrorMessage.FailStrings
    open System.IO

    let generateArcFileSystemTests (arcConfig: ArcConfig) =

        let pathConfig = arcConfig.PathConfig

        testList "Filesystem" [
            testCase "arcFolder"                    <| fun () -> Validate.Critical.FilesystemEntry.folder pathConfig.DotArcFolderPath   |> throwError FilesystemEntry.isPresent
            testCase "InvestigationFile"            <| fun () -> Validate.Critical.FilesystemEntry.file pathConfig.InvestigationPath    |> throwError FilesystemEntry.isPresent
            testCase "StudiesFolder"                <| fun () -> Validate.Critical.FilesystemEntry.folder pathConfig.StudiesPath        |> throwError FilesystemEntry.isPresent
            testCase "AssaysFolder"                 <| fun () -> Validate.Critical.FilesystemEntry.folder pathConfig.AssaysPath         |> throwError FilesystemEntry.isPresent
            testList "Git" [
                testCase "gitFolder"                <| fun () -> Validate.Critical.FilesystemEntry.folder pathConfig.GitFolderPath      |> throwError FilesystemEntry.isPresent
                //testCase "hooksFolder"              <| fun () -> Validate.FilesystemEntry.folder pathConfig.HooksPath          |> throwError FilesystemEntry.isPresent
                //testList "hooksFolder" [
                //    testCase "applyPatchFile"       <| fun () -> Validate.FilesystemEntry.file pathConfig.ApplyPatchPath       |> throwError FilesystemEntry.isPresent
                //    testCase "commitSampleFile"     <| fun () -> Validate.FilesystemEntry.file pathConfig.CommitSamplePath     |> throwError FilesystemEntry.isPresent
                //    testCase "fsmonitorFile"        <| fun () -> Validate.FilesystemEntry.file pathConfig.FsmonitorPath        |> throwError FilesystemEntry.isPresent
                //    testCase "postUpdateFile"       <| fun () -> Validate.FilesystemEntry.file pathConfig.PostUpdatePath       |> throwError FilesystemEntry.isPresent
                //    testCase "preApplyPatchFile"    <| fun () -> Validate.FilesystemEntry.file pathConfig.PreApplyPatchPath    |> throwError FilesystemEntry.isPresent
                //    testCase "preCommitFile"        <| fun () -> Validate.FilesystemEntry.file pathConfig.PreCommitPath        |> throwError FilesystemEntry.isPresent
                //    testCase "preMergeCommitFile"   <| fun () -> Validate.FilesystemEntry.file pathConfig.PreMergeCommitPath   |> throwError FilesystemEntry.isPresent
                //    testCase "prePushFile"          <| fun () -> Validate.FilesystemEntry.file pathConfig.PrePushPath          |> throwError FilesystemEntry.isPresent
                //    testCase "preRebaseFile"        <| fun () -> Validate.FilesystemEntry.file pathConfig.PreRebasePath        |> throwError FilesystemEntry.isPresent
                //    testCase "preReceiveFile"       <| fun () -> Validate.FilesystemEntry.file pathConfig.PreReceivePath       |> throwError FilesystemEntry.isPresent
                //    testCase "prepareCommitFile"    <| fun () -> Validate.FilesystemEntry.file pathConfig.PrepareCommitPath    |> throwError FilesystemEntry.isPresent
                //    testCase "pushToCheckoutFile"   <| fun () -> Validate.FilesystemEntry.file pathConfig.PushToCheckoutPath   |> throwError FilesystemEntry.isPresent
                //    testCase "updateFile"           <| fun () -> Validate.FilesystemEntry.file pathConfig.UpdatePath           |> throwError FilesystemEntry.isPresent
                //]
                //testCase "infoFolder"               <| fun () -> Validate.FilesystemEntry.folder pathConfig.InfoPath           |> throwError FilesystemEntry.isPresent
                //testList "infoFolder" [
                //    testCase "excludeFile"          <| fun () -> Validate.FilesystemEntry.file pathConfig.ExcludePath          |> throwError FilesystemEntry.isPresent
                //]
                //testCase "packFolder"               <| fun () -> Validate.FilesystemEntry.folder pathConfig.ObjectsPackPath    |> throwError FilesystemEntry.isPresent
                //testCase "refsFolder"               <| fun () -> Validate.FilesystemEntry.folder pathConfig.RefsPath           |> throwError FilesystemEntry.isPresent
                //testCase "refsHeadsFolder"          <| fun () -> Validate.FilesystemEntry.folder pathConfig.RefsHeadsPath      |> throwError FilesystemEntry.isPresent
                //testCase "refsTagsFolder"           <| fun () -> Validate.FilesystemEntry.folder pathConfig.RefsTagsPath       |> throwError FilesystemEntry.isPresent
            ]
            testList "Studies" [
                for (p,id) in arcConfig.StudyPathsAndIds do
                    // Validate every present Study filepath in Investigation for presence in the ARC filesystem:
                    if p.IsSome then
                        let defId = Option.defaultValue "(no Study identifier)" id
                        testCase $"{defId}" <| fun () -> Validate.Critical.FilesystemEntry.file p.Value |> throwError FilesystemEntry.isPresent
                for (p,id) in arcConfig.StudyFilesAndIds do
                    // Validate every Study in the ARC filesystem that has no Study file: (outcome will always be Error)
                    if p.IsNone then
                        let assumedFilepath = Path.Combine(pathConfig.StudiesPath, id, "isa.study.xlsx")
                        testCase $"{id}" <| fun () -> Validate.Critical.FilesystemEntry.file assumedFilepath |> throwError FilesystemEntry.isPresent
            ]
            testList "Assays" [
                for (p,id) in arcConfig.AssayFilesAndIds do
                    // Validate every Assay in the ARC filesystem that has no Assay file: (outcome will always be Error)
                    if p.IsNone then
                        let assumedFilepath = Path.Combine(pathConfig.StudiesPath, id, "isa.assay.xlsx")
                        testCase $"{id}" <| fun () -> Validate.Critical.FilesystemEntry.file assumedFilepath |> throwError FilesystemEntry.isPresent
            ]
            testList "DataPathNames" [
                for fp in arcConfig.DataPaths do
                    let fpParam = Param.tryParam fp |> Option.get
                    let fpValue = Param.getValueAsString fpParam
                    testCase $"{fpValue}" <| fun () -> Validate.Critical.Param.filepath pathConfig.ArcRootPath fpParam |> throwError XLSXFile.isPresent
            ]
        ]