module PathConfigTests

open ARCExpect
open ARCExpect.Configs

open Expecto

let pathFixture rootPath =
    fun f ->
        let pathConfig = PathConfig(rootPath)
        f pathConfig

let normalizePath (path: string) = path.Replace("\\","/")

[<Tests>]
let ``PathConfig tests`` =
    testList "PathConfig" [
        yield! testFixture (pathFixture "/arcs/test/path") [
            "PathConfig.ArcRootPath"       , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.ARCRootPath       ) "/arcs/test/path" "ArcRootPath was incorrect")
            "PathConfig.StudiesPath"       , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.StudiesPath       ) "/arcs/test/path/studies" "StudiesPath was incorrect")
            "PathConfig.AssaysPath"        , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.AssaysPath        ) "/arcs/test/path/assays" "AssaysPath was incorrect")
            "PathConfig.RunsPath"          , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.RunsPath          ) "/arcs/test/path/runs" "RunsPath was incorrect")
            "PathConfig.WorkflowsPath"     , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.WorkflowsPath     ) "/arcs/test/path/workflows" "WorkflowsPath was incorrect")
            "PathConfig.InvestigationPath" , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.InvestigationPath ) "/arcs/test/path/isa.investigation.xlsx" "InvestigationPath was incorrect")
            "PathConfig.DotArcFolderPath"  , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.DotARCFolderPath  ) "/arcs/test/path/.arc" "DotArcFolderPath was incorrect")
            "PathConfig.GitFolderPath"     , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.GitFolderPath     ) "/arcs/test/path/.git" "GitFolderPath was incorrect")
            "PathConfig.ConfigPath"        , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.ConfigPath        ) "/arcs/test/path/.git/config" "ConfigPath was incorrect")
            "PathConfig.DescriptionPath"   , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.DescriptionPath   ) "/arcs/test/path/.git/description" "DescriptionPath was incorrect")
            "PathConfig.HeadPath"          , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.HeadPath          ) "/arcs/test/path/.git/HEAD" "HeadPath was incorrect")
            "PathConfig.ObjectsPath"       , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.ObjectsPath       ) "/arcs/test/path/.git/objects" "ObjectsPath was incorrect")
            "PathConfig.ObjectsInfoPath"   , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.ObjectsInfoPath   ) "/arcs/test/path/.git/objects/info" "ObjectsInfoPath was incorrect")
            "PathConfig.ObjectsPackPath"   , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.ObjectsPackPath   ) "/arcs/test/path/.git/objects/pack" "ObjectsPackPath was incorrect")
            "PathConfig.InfoPath"          , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.InfoPath          ) "/arcs/test/path/.git/info" "InfoPath was incorrect")
            "PathConfig.ExcludePath"       , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.ExcludePath       ) "/arcs/test/path/.git/info/exclude" "ExcludePath was incorrect")
            "PathConfig.RefsPath"          , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.RefsPath          ) "/arcs/test/path/.git/refs" "RefsPath was incorrect")
            "PathConfig.RefsHeadsPath"     , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.RefsHeadsPath     ) "/arcs/test/path/.git/refs/heads" "RefsHeadsPath was incorrect")
            "PathConfig.RefsTagsPath"      , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.RefsTagsPath      ) "/arcs/test/path/.git/refs/tags" "RefsTagsPath was incorrect")
            "PathConfig.HooksPath"         , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.HooksPath         ) "/arcs/test/path/.git/hooks" "HooksPath was incorrect")
            "PathConfig.ApplyPatchPath"    , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.ApplyPatchPath    ) "/arcs/test/path/.git/hooks/applypatch-msg.sample" "ApplyPatchPath was incorrect")
            "PathConfig.CommitSamplePath"  , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.CommitSamplePath  ) "/arcs/test/path/.git/hooks/commit-msg.sample" "CommitSamplePath was incorrect")
            "PathConfig.FsmonitorPath"     , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.FsmonitorPath     ) "/arcs/test/path/.git/hooks/fsmonitor-watchman.sample" "FsmonitorPath was incorrect")
            "PathConfig.PostUpdatePath"    , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.PostUpdatePath    ) "/arcs/test/path/.git/hooks/post-update.sample" "PostUpdatePath was incorrect")
            "PathConfig.PreApplyPatchPath" , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.PreApplyPatchPath ) "/arcs/test/path/.git/hooks/pre-applypatch.sample" "PreApplyPatchPath was incorrect")
            "PathConfig.PreCommitPath"     , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.PreCommitPath     ) "/arcs/test/path/.git/hooks/pre-commit.sample" "PreCommitPath was incorrect")
            "PathConfig.PreMergeCommitPath", (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.PreMergeCommitPath) "/arcs/test/path/.git/hooks/pre-merge-commit.sample" "PreMergeCommitPath was incorrect")
            "PathConfig.PrePushPath"       , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.PrePushPath       ) "/arcs/test/path/.git/hooks/pre-push.sample" "PrePushPath was incorrect")
            "PathConfig.PreRebasePath"     , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.PreRebasePath     ) "/arcs/test/path/.git/hooks/pre-rebase.sample" "PreRebasePath was incorrect")
            "PathConfig.PreReceivePath"    , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.PreReceivePath    ) "/arcs/test/path/.git/hooks/pre-receive.sample" "PreReceivePath was incorrect")
            "PathConfig.PrepareCommitPath" , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.PrepareCommitPath ) "/arcs/test/path/.git/hooks/prepare-commit-msg.sample" "PrepareCommitPath was incorrect")
            "PathConfig.PushToCheckoutPath", (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.PushToCheckoutPath) "/arcs/test/path/.git/hooks/push-to-checkout.sample" "PushToCheckoutPath was incorrect")
            "PathConfig.UpdatePath"        , (fun pathConfig -> fun () -> Expect.equal (normalizePath pathConfig.UpdatePath        ) "/arcs/test/path/.git/hooks/update.sample" "UpdatePath was incorrect")
        ]
    ]
   