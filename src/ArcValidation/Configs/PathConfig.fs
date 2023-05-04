/// Paths to folders and files that must/should exist in an ARC.
namespace ArcValidation.Configs

open System.IO

type PathConfig(arcRootPath: string) = 

    // settable root path. All other properties are explicit properties meaning that they get evaluated everytime they are accessed -> they are always changed accordingly to the root path. 
    // Useful for setting correct root path via CLI args or defaulting to another value.
    member val ArcRootPath = arcRootPath with get, set

    //
    member this.StudiesPath         = Path.Combine(this.ArcRootPath, "studies")
    member this.AssaysPath          = Path.Combine(this.ArcRootPath, "assays")
    member this.RunsPath            = Path.Combine(this.ArcRootPath, "runs")
    member this.WorkflowsPath       = Path.Combine(this.ArcRootPath, "workflows")
    member this.InvestigationPath   = Path.Combine(this.ArcRootPath, "isa.investigation.xlsx")

    //
    member this.DotArcFolderPath    = Path.Combine(this.ArcRootPath, ".arc")

    //
    member this.GitFolderPath       = Path.Combine(this.ArcRootPath, ".git")
    member this.ConfigPath          = Path.Combine(this.GitFolderPath, "config")
    member this.DescriptionPath     = Path.Combine(this.GitFolderPath, "description")
    member this.HeadPath            = Path.Combine(this.GitFolderPath, "HEAD")

    //
    member this.ObjectsPath         = Path.Combine(this.GitFolderPath, "objects")
    member this.ObjectsInfoPath     = Path.Combine(this.ObjectsPath, "info")
    member this.ObjectsPackPath     = Path.Combine(this.ObjectsPath, "pack")

    //
    member this.InfoPath            = Path.Combine(this.GitFolderPath, "info")
    member this.ExcludePath         = Path.Combine(this.InfoPath, "exclude")

    //
    member this.RefsPath            = Path.Combine(this.GitFolderPath, "refs")
    member this.RefsHeadsPath       = Path.Combine(this.RefsPath, "heads")
    member this.RefsTagsPath        = Path.Combine(this.RefsPath, "tags")

    //
    member this.HooksPath           = Path.Combine(this.GitFolderPath, "hooks")
    member this.ApplyPatchPath      = Path.Combine(this.HooksPath, "applypatch-msg.sample")
    member this.CommitSamplePath    = Path.Combine(this.HooksPath, "commit-msg.sample")
    member this.FsmonitorPath       = Path.Combine(this.HooksPath, "fsmonitor-watchman.sample")
    member this.PostUpdatePath      = Path.Combine(this.HooksPath, "post-update.sample")
    member this.PreApplyPatchPath   = Path.Combine(this.HooksPath, "pre-applypatch.sample")
    member this.PreCommitPath       = Path.Combine(this.HooksPath, "pre-commit.sample")
    member this.PreMergeCommitPath  = Path.Combine(this.HooksPath, "pre-merge-commit.sample")
    member this.PrePushPath         = Path.Combine(this.HooksPath, "pre-push.sample")
    member this.PreRebasePath       = Path.Combine(this.HooksPath, "pre-rebase.sample")
    member this.PreReceivePath      = Path.Combine(this.HooksPath, "pre-receive.sample")
    member this.PrepareCommitPath   = Path.Combine(this.HooksPath, "prepare-commit-msg.sample")
    member this.PushToCheckoutPath  = Path.Combine(this.HooksPath, "push-to-checkout.sample")
    member this.UpdatePath          = Path.Combine(this.HooksPath, "update.sample")

