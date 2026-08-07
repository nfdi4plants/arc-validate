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

let private createBuildTestsTarget name projectsToBuild =
    BuildTask.create name [clean; build] {
        projectsToBuild
        |> List.iter (fun pInfo ->
            let proj = pInfo.ProjFile
            proj
            |> DotNet.build (fun p ->
                {
                    p with
                        MSBuildParams = { p.MSBuildParams with DisableInternalBinLog = true}
                }
                |> DotNet.Options.withCustomParams (Some "-tl -m:1")
            )
        )
    }

let buildTests = createBuildTestsTarget "BuildTests" testProjects

let buildIntegrationTests =
    createBuildTestsTarget "BuildIntegrationTests" integrationTestProjects

let private runTestProjects projectsToRun =
    projectsToRun
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

let runTests = BuildTask.create "RunTests" [clean; ensureArcFixtures; publish; buildTests; buildIntegrationTests] {
    runTestProjects testProjects
    runTestProjects integrationTestProjects
}

let runAutomatedTests = BuildTask.create "RunAutomatedTests" [clean; ensureArcFixtures; publish; buildTests] {
    runTestProjects testProjects
}

let runIntegrationTests =
    BuildTask.create "RunIntegrationTests" [clean; ensureArcFixtures; publish; buildIntegrationTests] {
        runTestProjects integrationTestProjects
}

