/// Paths to folders and files that must/should exist in an ARC.
module ArcPaths

open System.IO

type ArcPaths(arcRootPath: string) = 
    //
    member this.studiesPath         = Path.Combine(arcRootPath, "studies")
    member this.assaysPath          = Path.Combine(arcRootPath, "assays")
    member this.runsPath            = Path.Combine(arcRootPath, "runs")
    member this.workflowsPath       = Path.Combine(arcRootPath, "workflows")
    member this.investigationPath   = Path.Combine(arcRootPath, "isa.investigation.xlsx")
    //
    member this.dotArcFolderPath    = Path.Combine(arcRootPath, ".arc")

    //
    member this.gitFolderPath       = Path.Combine(arcRootPath, ".git")
    member this.configPath          = Path.Combine(this.gitFolderPath, "config")
    member this.descriptionPath     = Path.Combine(this.gitFolderPath, "description")
    member this.headPath            = Path.Combine(this.gitFolderPath, "HEAD")

    //
    member this.objectsPath         = Path.Combine(this.gitFolderPath, "objects")
    member this.objectsInfoPath     = Path.Combine(this.objectsPath, "info")
    member this.objectsPackPath     = Path.Combine(this.objectsPath, "pack")

    //
    member this.infoPath            = Path.Combine(this.gitFolderPath, "info")
    member this.excludePath         = Path.Combine(this.infoPath, "exclude")

    //
    member this.refsPath            = Path.Combine(this.gitFolderPath, "refs")
    member this.refsHeadsPath       = Path.Combine(this.refsPath, "heads")
    member this.refsTagsPath        = Path.Combine(this.refsPath, "tags")

    //
    member this.hooksPath           = Path.Combine(this.gitFolderPath, "hooks")
    member this.applyPatchPath      = Path.Combine(this.hooksPath, "applypatch-msg.sample")
    member this.commitSamplePath    = Path.Combine(this.hooksPath, "commit-msg.sample")
    member this.fsmonitorPath       = Path.Combine(this.hooksPath, "fsmonitor-watchman.sample")
    member this.postUpdatePath      = Path.Combine(this.hooksPath, "post-update.sample")
    member this.preApplyPatchPath   = Path.Combine(this.hooksPath, "pre-applypatch.sample")
    member this.preCommitPath       = Path.Combine(this.hooksPath, "pre-commit.sample")
    member this.preMergeCommitPath  = Path.Combine(this.hooksPath, "pre-merge-commit.sample")
    member this.prePushPath         = Path.Combine(this.hooksPath, "pre-push.sample")
    member this.preRebasePath       = Path.Combine(this.hooksPath, "pre-rebase.sample")
    member this.preReceivePath      = Path.Combine(this.hooksPath, "pre-receive.sample")
    member this.prepareCommitPath   = Path.Combine(this.hooksPath, "prepare-commit-msg.sample")
    member this.pushToCheckoutPath  = Path.Combine(this.hooksPath, "push-to-checkout.sample")
    member this.updatePath          = Path.Combine(this.hooksPath, "update.sample")
