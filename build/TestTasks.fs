module TestTasks

open BlackFox.Fake
open Fake.DotNet
open Fake.Tools.Git

open ProjectInfo
open BasicTasks
open System.IO

let ensureArcFixtures = BuildTask.create "EnsureArcFixtures" [] {
    let arcFixtures = [
        "tests/arc-validate.Tests/fixtures/arcs/inveniotestarc"
    ]
    arcFixtures
    |> List.iter (fun arcPath ->
        if not (Directory.Exists(arcPath)) then 
            Directory.CreateDirectory(arcPath) |> ignore
            printfn $"cloning {arcPath}"
            Repository.clone "tests/arc-validate.Tests/fixtures/arcs" "https://github.com/nfdi4plants/invenio-test-arc" "inveniotestarc"
    )
}

let buildTests = 
    BuildTask.create "BuildTests" [clean; build] {
        testProjects
        |> List.iter (fun pInfo ->
            let proj = pInfo.ProjFile
            proj
            |> DotNet.build (fun p ->
                {
                    p with
                        MSBuildParams = { p.MSBuildParams with DisableInternalBinLog = true}
                }
                |> DotNet.Options.withCustomParams (Some "-tl")
            )
        )
    }

let runTests = BuildTask.create "RunTests" [clean; ensureArcFixtures; publish; buildTests] {
    testProjects
    |> Seq.iter (fun testProjectInfo ->
        Fake.DotNet.DotNet.test
            (fun testParams ->
                { testParams with
                    Logger = Some "console;verbosity=detailed"
                    Configuration = DotNet.BuildConfiguration.fromString configuration
                    NoBuild = true
                    MSBuildParams = { testParams.MSBuildParams with DisableInternalBinLog = true }
                }
                |> DotNet.Options.withCustomParams (Some "-tl")
            )
            testProjectInfo.ProjFile
        )
}

