module ARCExpectPortableTasks

open BlackFox.Fake
open Fake.DotNet
open System
open System.IO

open Helpers
open ProjectInfo
open BasicTasks
open PackageTasks

let private testsDir = Path.Combine(portableArtifactsDir, "arcexpect-tests")
let private packageSmokeDir = Path.Combine(portableArtifactsDir, "arcexpect-package-smoke")
let private nativeDependencyPackageDirectory () =
    let configured = Environment.GetEnvironmentVariable "AVPR_NATIVE_PACKAGE_DIR"
    let directory =
        if String.IsNullOrWhiteSpace configured then
            Path.Combine("..", "arc-validate-package-registry", "artifacts", "packages")
            |> Path.GetFullPath
        else
            Path.GetFullPath configured

    if not (Directory.Exists directory) then
        failwithf
            "AVPR native package directory does not exist: %s. Build AVPR PackPortablePackages or set AVPR_NATIVE_PACKAGE_DIR."
            directory

    directory

let private nativeDependencyArtifact fileName =
    let path = Path.Combine(nativeDependencyPackageDirectory (), fileName)

    if not (File.Exists path) then
        failwithf
            "Required AVPR native package is missing: %s. Build AVPR PackPortablePackages first."
            path

    path

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
                (Path.Combine(
                    portableArtifactsDir,
                    "arcexpect-package-smoke-dotnet",
                    "NuGet.Config"
                ))
                [
                    packageDir
                    nativeDependencyPackageDirectory()
                ]

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

        runDotNetCommand
            "run"
            $"--project {ARCExpectPackageSmokeProject} --configuration Release --no-restore -- -i packed-arc -o packed-out --echo \"literal; $(not-executed)\""
            "."

        let fsharpSmokeSource =
            Path.Combine("tests", "ARCExpect.PackageSmoke", "fsharp.fsx")
            |> Path.GetFullPath

        let fsharpSmokeDirectory = Path.GetDirectoryName nugetConfig
        let fsharpSmokeScript = Path.Combine(fsharpSmokeDirectory, "fsharp.fsx")
        let localNuGetSource = Uri(Path.GetFullPath packageDir).AbsoluteUri
        File.WriteAllText(
            fsharpSmokeScript,
            $"#i \"nuget: {localNuGetSource}\"{Environment.NewLine}{File.ReadAllText fsharpSmokeSource}"
        )

        runDotNetCommand
            "fsi"
            $"\"{fsharpSmokeScript}\" -i packed-arc -o packed-out --echo \"literal; $(not-executed)\""
            fsharpSmokeDirectory

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
            Path.Combine(packageDir, $"nfdi4plants-arcexpect-{ARCExpectPackageVersion}.tgz")
            |> Path.GetFullPath
        let javaScriptModelPackage =
            nativeDependencyArtifact $"nfdi4plants-validationpackage-model-{ValidationPackageModelNativeVersion}.tgz"
        let javaScriptCodecsPackage =
            nativeDependencyArtifact $"nfdi4plants-validationpackage-codecs-{ValidationPackageCodecsNativeVersion}.tgz"
        runNpm
            [
                "install"
                javaScriptModelPackage
                javaScriptCodecsPackage
                javaScriptPackage
                "--no-audit"
                "--no-fund"
            ]
            javaScriptDirectory
        runCommand
            "node"
            [
                "javascript.mjs"
                "-i"
                "packed-arc"
                "-o"
                "packed-out"
                "--echo"
                "literal; $(not-executed)"
            ]
            javaScriptDirectory

        let pythonDirectory = Path.Combine(packageSmokeDir, "python")
        let pythonEnvironment = Path.Combine(pythonDirectory, ".venv")
        recreateDirectory pythonDirectory
        File.Copy(
            Path.Combine("tests", "ARCExpect.PackageSmoke", "python.py"),
            Path.Combine(pythonDirectory, "python.py")
        )
        let pythonPackageVersion =
            toPythonPackageVersion ARCExpectPackageVersion
        let pythonPackage =
            Path.Combine(packageDir, $"arcexpect-{pythonPackageVersion}-py3-none-any.whl")
            |> Path.GetFullPath
        runUv [ "venv"; pythonEnvironment ] "."
        let pythonExecutable =
            if OperatingSystem.IsWindows() then
                Path.Combine(pythonEnvironment, "Scripts", "python.exe")
            else
                Path.Combine(pythonEnvironment, "bin", "python")
        let pythonModelPackage =
            nativeDependencyArtifact $"validationpackage_model-{ValidationPackageModelPythonVersion}-py3-none-any.whl"
        let pythonCodecsPackage =
            nativeDependencyArtifact $"validationpackage_codecs-{ValidationPackageCodecsPythonVersion}-py3-none-any.whl"
        runUv
            [
                "pip"
                "install"
                "--python"
                pythonExecutable
                pythonModelPackage
                pythonCodecsPackage
                pythonPackage
            ]
            "."
        runCommand
            pythonExecutable
            [
                "python.py"
                "-i"
                "packed-arc"
                "-o"
                "packed-out"
                "--echo"
                "literal; $(not-executed)"
            ]
            pythonDirectory

    }

let testPortableARCExpect =
    BuildTask.createEmpty "TestPortableARCExpect" [ testARCExpectPackage ]
