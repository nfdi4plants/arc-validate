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
let private localNativeDependencyPackageDirectory () =
    let configured = Environment.GetEnvironmentVariable "AVPR_NATIVE_PACKAGE_DIR"

    if String.IsNullOrWhiteSpace configured then
        let siblingDirectory =
            Path.Combine("..", "arc-validate-package-registry", "artifacts", "packages")
            |> Path.GetFullPath

        if Directory.Exists siblingDirectory then
            Some siblingDirectory
        else
            None
    else
        let configuredDirectory = Path.GetFullPath configured

        if not (Directory.Exists configuredDirectory) then
            failwithf
                "AVPR_NATIVE_PACKAGE_DIR does not exist: %s"
                configuredDirectory

        Some configuredDirectory

let private localNativeDependencyArtifact directory fileName =
    let path = Path.Combine(directory, fileName)

    if not (File.Exists path) then
        failwithf
            "Required AVPR native package is missing: %s. Build AVPR PackPortablePackages first."
            path

    path

let private fable project language outputDirectory noRestore =
    let restoreArgument = if noRestore then " --noRestore" else ""
    runDotNetCommand "fable" $"{project} --outDir \"{outputDirectory}\" --lang {language} --noCache{restoreArgument}" "."

let testARCExpectDotNet =
    BuildTask.create
        "TestARCExpectDotNet"
        [ cleanPortableArtifacts; preparePortableToolchain; packARCExpect ] {
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
    BuildTask.create "TestARCExpectPackage" [ testARCExpectPython ] {
        let localNativePackageDirectory = localNativeDependencyPackageDirectory ()
        let cacheDirectory = Path.Combine(packageCacheDir, "arcexpect") |> Path.GetFullPath
        let nugetConfig =
            writeNuGetConfig
                (Path.Combine(
                    portableArtifactsDir,
                    "arcexpect-package-smoke-dotnet",
                    "NuGet.Config"
                ))
                ([ packageDir ] @ (localNativePackageDirectory |> Option.toList))

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
        let versionedFsharpSmokeSource =
            File.ReadAllText(fsharpSmokeSource)
                .Replace(
                    "__ARCEXPECT_VERSION__",
                    ARCExpectPackageVersion
                )
                .Replace(
                    "__ARCEXPECT_SOURCE__",
                    localNuGetSource
                )
        File.WriteAllText(
            fsharpSmokeScript,
            versionedFsharpSmokeSource
        )

        runCommandWithEnvironmentVariable
            "dotnet"
            [
                "fsi"
                fsharpSmokeScript
                "-i"
                "packed-arc"
                "-o"
                "packed-out"
                "--echo"
                "literal; $(not-executed)"
            ]
            fsharpSmokeDirectory
            "NUGET_PACKAGES"
            cacheDirectory

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
        let javaScriptDependencyPackages =
            match localNativePackageDirectory with
            | Some directory ->
                [
                    localNativeDependencyArtifact
                        directory
                        $"nfdi4plants-validationpackage-model-{ValidationPackageModelNativeVersion}.tgz"
                    localNativeDependencyArtifact
                        directory
                        $"nfdi4plants-validationpackage-codecs-{ValidationPackageCodecsNativeVersion}.tgz"
                ]
            | None -> []

        runNpm
            ([ "install" ]
             @ javaScriptDependencyPackages
             @ [ javaScriptPackage; "--no-audit"; "--no-fund" ])
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
        let pythonDependencyPackages =
            match localNativePackageDirectory with
            | Some directory ->
                [
                    localNativeDependencyArtifact
                        directory
                        $"validationpackage_model-{ValidationPackageModelPythonVersion}-py3-none-any.whl"
                    localNativeDependencyArtifact
                        directory
                        $"validationpackage_codecs-{ValidationPackageCodecsPythonVersion}-py3-none-any.whl"
                ]
            | None -> []

        runUv
            ([ "pip"; "install"; "--python"; pythonExecutable ]
             @ pythonDependencyPackages
             @ [ pythonPackage ])
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
