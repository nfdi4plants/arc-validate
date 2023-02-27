module CheckFilesystemStructure

open FilesystemRepresentation
open System.IO


module Paths =

    //let inputPath           = Directory.GetCurrentDirectory()
    let inputPath           = System.Environment.GetEnvironmentVariable("ARC_PATH")

    let arcFolderPath       = Path.Combine(inputPath, ".arc")
    let gitFolderPath       = Path.Combine(inputPath, ".git")
    let hooksPath           = Path.Combine(gitFolderPath, "hooks")
    let applyPatchPath      = Path.Combine(hooksPath, "applypatch-msg.sample")
    let commitSamplePath    = Path.Combine(hooksPath, "commit-msg.sample")
    let fsmonitorPath       = Path.Combine(hooksPath, "fsmonitor-watchman.sample")
    let postUpdatePath      = Path.Combine(hooksPath, "post-update.sample")
    let preApplyPatchPath   = Path.Combine(hooksPath, "pre-applypatch.sample")
    let preCommitPath       = Path.Combine(hooksPath, "pre-commit.sample")
    let preMergeCommitPath  = Path.Combine(hooksPath, "pre-merge-commit.sample")
    let prePushPath         = Path.Combine(hooksPath, "pre-push.sample")
    let preRebasePath       = Path.Combine(hooksPath, "pre-rebase.sample")
    let preReceivePath      = Path.Combine(hooksPath, "pre-receive.sample")
    let prepareCommitPath   = Path.Combine(hooksPath, "prepare-commit-msg.sample")
    let pushToCheckoutPath  = Path.Combine(hooksPath, "push-to-checkout.sample")
    let updatePath          = Path.Combine(hooksPath, "update.sample")
    let infoPath            = Path.Combine(gitFolderPath, "info")
    let excludePath         = Path.Combine(infoPath, "exclude")
    let objectsPath         = Path.Combine(gitFolderPath, "objects")
    let objectsInfoPath     = Path.Combine(objectsPath, "info")
    let objectsPackPath     = Path.Combine(objectsPath, "pack")
    let refsPath            = Path.Combine(gitFolderPath, "refs")
    let refsHeadsPath       = Path.Combine(refsPath, "heads")
    let refsTagsPath        = Path.Combine(refsPath, "tags")
    let configPath          = Path.Combine(gitFolderPath, "config")
    let descriptionPath     = Path.Combine(gitFolderPath, "description")
    let headPath            = Path.Combine(gitFolderPath, "HEAD")
    let studiesPath         = Path.Combine(inputPath, "studies")
    let assaysPath          = Path.Combine(inputPath, "assays")
    let runsPath            = Path.Combine(inputPath, "runs")
    let workflowsPath       = Path.Combine(inputPath, "workflows")
    let investigationPath   = Path.Combine(inputPath, "isa.investigation.xlsx")


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