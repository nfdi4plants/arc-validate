module ARCExpectPortableTasks

open BlackFox.Fake
open Fake.DotNet
open System.IO

open Helpers
open ProjectInfo
open BasicTasks
open PackageTasks

let private testsDir = Path.Combine(portableArtifactsDir, "arcexpect-tests")
let private packageSmokeDir = Path.Combine(portableArtifactsDir, "arcexpect-package-smoke")
let private fable project language outputDirectory noRestore =
    let restoreArgument = if noRestore then " --noRestore" else ""
    runDotNetCommand "fable" $"{project} --outDir \"{outputDirectory}\" --lang {language} --noCache{restoreArgument}" "."

let testARCExpectDotNet =
    BuildTask.create "TestARCExpectDotNet" [ cleanPortableArtifacts; preparePortableToolchain ] {
        runDotNetCommand "run" $"--project {ARCExpectContractTestsProject} --configuration Release" "."
    }

let testARCExpectJavaScript =
    BuildTask.create "TestARCExpectJavaScript" [ testARCExpectDotNet ] {
        let outputDirectory = Path.Combine(testsDir, "javascript")
        fable ARCExpectJavaScriptTestsProject "javascript" outputDirectory false
        runCommand "node" [ Path.Combine(outputDirectory, "Main.js") ] "."
    }

let testARCExpectPython =
    BuildTask.create "TestARCExpectPython" [ testARCExpectJavaScript ] {
        let outputDirectory = Path.Combine(testsDir, "python")
        fable ARCExpectPythonTestsProject "python" outputDirectory false
        runUv [ "run"; "--locked"; "python"; Path.Combine(outputDirectory, "main.py") ] "."
    }

let testARCExpectPackage =
    BuildTask.create "TestARCExpectPackage" [ testARCExpectPython; packARCExpect ] {
        let cacheDirectory = Path.Combine(packageCacheDir, "arcexpect") |> Path.GetFullPath
        let nugetConfig =
            writeNuGetConfig
                (Path.Combine(portableArtifactsDir, "arcexpect-package-smoke.NuGet.config"))
                packageDir

        recreateDirectory cacheDirectory

        ARCExpectPackageSmokeProject
        |> DotNet.restore (fun options ->
            { options with
                ConfigFile = Some nugetConfig
                Packages = [ cacheDirectory ]
                NoCache = true
                MSBuildParams =
                    { options.MSBuildParams with
                        DisableInternalBinLog = true } })

        runDotNetCommand "run" $"--project {ARCExpectPackageSmokeProject} --configuration Release --no-restore" "."

        let javaScriptDirectory = Path.Combine(packageSmokeDir, "javascript")
        recreateDirectory javaScriptDirectory
        File.Copy(
            Path.Combine("tests", "ARCExpect.PackageSmoke", "javascript.mjs"),
            Path.Combine(javaScriptDirectory, "javascript.mjs")
        )
        File.WriteAllText(
            Path.Combine(javaScriptDirectory, "package.json"),
            "{ \"name\": \"arcexpect-smoke\", \"private\": true, \"type\": \"module\" }"
        )
        let javaScriptPackage =
            Path.Combine(packageDir, $"arcexpect-{ARCExpectPackageVersion}.tgz")
            |> Path.GetFullPath
        runNpm [ "install"; javaScriptPackage; "--no-audit"; "--no-fund" ] javaScriptDirectory
        runCommand "node" [ "javascript.mjs" ] javaScriptDirectory

        let pythonDirectory = Path.Combine(packageSmokeDir, "python")
        let pythonEnvironment = Path.Combine(pythonDirectory, ".venv")
        recreateDirectory pythonDirectory
        File.Copy(
            Path.Combine("tests", "ARCExpect.PackageSmoke", "python.py"),
            Path.Combine(pythonDirectory, "python.py")
        )
        let pythonPackageVersion =
            ARCExpectPackageVersion
                .Replace("-alpha.", "a")
                .Replace("-beta.", "b")
                .Replace("-rc.", "rc")
        let pythonPackage =
            Path.Combine(packageDir, $"arcexpect-{pythonPackageVersion}-py3-none-any.whl")
            |> Path.GetFullPath
        runUv [ "venv"; pythonEnvironment ] "."
        runUv [ "pip"; "install"; "--python"; pythonEnvironment; pythonPackage ] "."
        let pythonExecutable =
            if System.OperatingSystem.IsWindows() then
                Path.Combine(pythonEnvironment, "Scripts", "python.exe")
            else
                Path.Combine(pythonEnvironment, "bin", "python")
        runCommand pythonExecutable [ "python.py" ] pythonDirectory

    }

let testPortableARCExpect =
    BuildTask.createEmpty "TestPortableARCExpect" [ testARCExpectPackage ]
