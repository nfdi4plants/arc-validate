namespace Validate

open System.IO
open ErrorMessage
open OntologyHelperFunctions


// [DEPRECATED]
// temporarily stays here to remember which cases to cover
module Checks =

    open ArcPaths

    let hasGitFolder            = Directory.Exists gitFolderPath
    let hasHooksFolder          = Directory.Exists hooksPath
    let hasApplyPatchFile       = File.Exists applyPatchPath
    let hasCommitSampleFile     = File.Exists commitSamplePath
    let hasFsMonitorFile        = File.Exists fsmonitorPath
    let hasPostUpdateFile       = File.Exists postUpdatePath
    let hasPreAppyPatchFile     = File.Exists preApplyPatchPath
    let hasPreCommitFile        = File.Exists preCommitPath
    let hasPreMergeCommitFile   = File.Exists preMergeCommitPath
    let hasPrePushFile          = File.Exists prePushPath
    let hasPreRebaseFile        = File.Exists preRebasePath
    let hasPreReceiveFile       = File.Exists preReceivePath
    let hasPrepareCommitFile    = File.Exists prepareCommitPath
    let hasPushToCheckoutFile   = File.Exists pushToCheckoutPath
    let hasUpdateFile           = File.Exists updatePath
    let hasInfoFolder           = Directory.Exists infoPath
    let hasObjectsFolder        = Directory.Exists objectsPath
    let hasRefsFolder           = Directory.Exists refsPath
    let hasRunsFolder           = Directory.Exists runsPath
    let hasWorkflowsFolder      = Directory.Exists workflowsPath
    let hasInvestigationFile    = File.Exists investigationPath
    //let studiesFolderStructure  = getElementInElementsFolder studiesPath
    //let allStudies              = checkStudiesFolderStructure studiesFolderStructure


module FilesystemEntry =

    /// Generalized function: Validates a folderpath.
    let private folder folderpath =
        let message = Message.create folderpath None None None MessageKind.FilesystemEntryKind
        if Directory.Exists folderpath then Success
        else Error message

    /// Generalized function: Validates a filepath.
    let private file filepath =
        let message = Message.create filepath None None None MessageKind.FilesystemEntryKind
        if File.Exists filepath then Success
        else Error message

    /// Validates a .arc folderpath.
    let dotArcFolder dotArcPath = 
        folder dotArcPath

    /// Validates a studies folderpath.
    let studiesFolder studiesFolderpath =
        folder studiesFolderpath

    /// Validates a Study's folderpath.
    let studyFolder studyFolderpath =
        folder studyFolderpath

    /// Validates an assays folderpath.
    let assaysFolder assaysFolderpath =
        folder assaysFolderpath

    /// Validates an Assay's folderpath.
    let assayFolder assayFolderpath =
        folder assayFolderpath

    /// Validates an Investigation folderpath.
    let investigationFile investigationFilepath =
        file investigationFilepath