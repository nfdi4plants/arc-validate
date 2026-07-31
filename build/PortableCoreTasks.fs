module PortableCoreTasks

open BlackFox.Fake
open Fake.DotNet
open System.IO

open Helpers
open ProjectInfo
open BasicTasks
open PackageTasks

let private testsDir = Path.Combine(portableArtifactsDir, "arcexpect-core-tests")
let private packageSmokeDir = Path.Combine(portableArtifactsDir, "arcexpect-core-package-smoke")

let private fable project language outputDirectory noRestore =
    let restoreArgument = if noRestore then " --noRestore" else ""
    runDotNetCommand "fable" $"{project} --outDir \"{outputDirectory}\" --lang {language} --noCache{restoreArgument}" "."

let testARCExpectCoreDotNet =
    BuildTask.create "TestARCExpectCoreDotNet" [ cleanPortableArtifacts; preparePortableToolchain ] {
        runDotNetCommand "run" $"--project {ARCExpectCorePortableTestsProject} --configuration Release" "."
    }

let testARCExpectCoreJavaScript =
    BuildTask.create "TestARCExpectCoreJavaScript" [ testARCExpectCoreDotNet ] {
        let outputDirectory = Path.Combine(testsDir, "javascript")
        fable ARCExpectCorePortableTestsProject "javascript" outputDirectory false
        runCommand "node" [ Path.Combine(outputDirectory, "Main.js") ] "."
    }

let testARCExpectCorePython =
    BuildTask.create "TestARCExpectCorePython" [ testARCExpectCoreJavaScript ] {
        let outputDirectory = Path.Combine(testsDir, "python")
        fable ARCExpectCorePortableTestsProject "python" outputDirectory false
        runUv [ "run"; "--locked"; "python"; Path.Combine(outputDirectory, "main.py") ] "."
    }

let testARCExpectCorePackage =
    BuildTask.create "TestARCExpectCorePackage" [ testARCExpectCorePython; packARCExpectCorePortable ] {
        let cacheDirectory = Path.Combine(packageCacheDir, "arcexpect-core") |> Path.GetFullPath
        let nugetConfig =
            writeNuGetConfig
                (Path.Combine(portableArtifactsDir, "arcexpect-core-package-smoke.NuGet.config"))
                packageDir

        recreateDirectory cacheDirectory

        ARCExpectCorePortablePackageSmokeProject
        |> DotNet.restore (fun options ->
            { options with
                ConfigFile = Some nugetConfig
                Packages = [ cacheDirectory ]
                NoCache = true
                MSBuildParams =
                    { options.MSBuildParams with
                        DisableInternalBinLog = true } })

        runDotNetCommand "run" $"--project {ARCExpectCorePortablePackageSmokeProject} --configuration Release --no-restore" "."

        let javaScriptOutput = Path.Combine(packageSmokeDir, "javascript")
        fable ARCExpectCorePortablePackageSmokeProject "javascript" javaScriptOutput true
        runCommand "node" [ Path.Combine(javaScriptOutput, "Program.js") ] "."

        let pythonOutput = Path.Combine(packageSmokeDir, "python")
        fable ARCExpectCorePortablePackageSmokeProject "python" pythonOutput true
        runUv [ "run"; "--locked"; "python"; Path.Combine(pythonOutput, "program.py") ] "."
    }

let testPortableARCExpectCore =
    BuildTask.createEmpty "TestPortableARCExpectCore" [ testARCExpectCorePackage ]
