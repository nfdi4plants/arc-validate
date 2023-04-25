module ValidateFilesystemEntries

open FilesystemRepresentation
open FilesystemStructure
open System.IO


module Checks =

    open Paths

    let hasArcFolder            = Directory.Exists arcFolderPath
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
    let hasStudiesFolder        = Directory.Exists studiesPath
    let hasAssaysFolder         = Directory.Exists assaysPath
    let hasRunsFolder           = Directory.Exists runsPath
    let hasWorkflowsFolder      = Directory.Exists workflowsPath
    let hasInvestigationFile    = File.Exists investigationPath
    let studiesFolderStructure  = getElementInElementsFolder studiesPath
    let allStudies              = checkStudiesFolderStructure studiesFolderStructure


let arcFolder = 0