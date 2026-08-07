module PackageTasks

open ProjectInfo

open BasicTasks

open BlackFox.Fake
open Fake.DotNet
open Helpers
open System.IO
open System.Text.RegularExpressions

let cleanPortablePackages = BuildTask.create "CleanPortablePackages" [] {
    recreateDirectory packageDir
}

let private packPortableProject project =
    ensureDirectory packageDir

    let restoredLocally =
        match localNativeDependencyPackageDirectory () with
        | Some directory ->
            let nugetConfig =
                writeNuGetConfig
                    (Path.Combine(portableArtifactsDir, "arcexpect-pack", "NuGet.Config"))
                    [ directory ]

            project
            |> DotNet.restore (fun options ->
                { options with
                    ConfigFile = Some nugetConfig
                    NoCache = true
                    MSBuildParams =
                        { options.MSBuildParams with
                            DisableInternalBinLog = true } })

            true
        | None -> false

    project
    |> DotNet.pack (fun options ->
        { options with
            Configuration = DotNet.BuildConfiguration.Release
            OutputPath = Some packageDir
            NoRestore = restoredLocally
            MSBuildParams =
                { options.MSBuildParams with
                    DisableInternalBinLog = true } })

let private removeFableModulesGitIgnore outputDirectory =
    let path = Path.Combine(outputDirectory, "fable_modules", ".gitignore")

    if File.Exists path then
        File.Delete path

let private transpileARCExpect project language outputDirectory =
    recreateDirectory outputDirectory
    runDotNetCommand "fable" $"{project} --lang {language} --outDir \"{outputDirectory}\" --noCache" "."
    removeFableModulesGitIgnore outputDirectory

let private rewriteFiles outputDirectory (pattern: string) (replacement: string) =
    Directory.EnumerateFiles(outputDirectory, "*", SearchOption.AllDirectories)
    |> Seq.filter (fun path -> path.EndsWith(".js") || path.EndsWith(".py"))
    |> Seq.iter (fun path ->
        let source = File.ReadAllText path
        let rewritten = Regex.Replace(source, pattern, replacement)

        if rewritten <> source then
            File.WriteAllText(path, rewritten)
    )

let private deleteMatchingDirectories parentDirectory pattern =
    if Directory.Exists parentDirectory then
        Directory.EnumerateDirectories(parentDirectory, pattern)
        |> Seq.iter (fun path -> Directory.Delete(path, true))

let private externalizeARCExpectJavaScriptDependencies outputDirectory =
    rewriteFiles
        outputDirectory
        "\"(?:\\.\\./|\\./)+fable_modules/ValidationPackage\\.Model\\.[^/]+/([^\"]+)\\.fs\\.js\""
        "\"@nfdi4plants/validationpackage-model/$1.js\""
    rewriteFiles
        outputDirectory
        "\"(?:\\.\\./|\\./)+fable_modules/ValidationPackage\\.Codecs\\.[^/]+/([^\"]+)\\.fs\\.js\""
        "\"@nfdi4plants/validationpackage-codecs/$1.js\""
    rewriteFiles
        outputDirectory
        "@nfdi4plants/validationpackage-codecs/JsonRuntime\.js"
        "@nfdi4plants/validationpackage-codecs/Json/Runtime.js"
    rewriteFiles
        outputDirectory
        "@nfdi4plants/validationpackage-codecs/ValidationPackageYaml\.js"
        "@nfdi4plants/validationpackage-codecs/Yaml/ValidationPackageYaml.js"

    let fableModules = Path.Combine(outputDirectory, "fable_modules")
    deleteMatchingDirectories fableModules "ValidationPackage.Model.*"
    deleteMatchingDirectories fableModules "ValidationPackage.Codecs.*"

let private externalizeARCExpectPythonDependencies packageDirectory =
    rewriteFiles
        packageDirectory
        @"from \.+fable_modules\.validation_package_model\."
        "from validation_package_model."
    rewriteFiles
        packageDirectory
        @"from \.+fable_modules\.validation_package_codecs\."
        "from validation_package_codecs."
    rewriteFiles
        packageDirectory
        @"from validation_package_codecs\.json_runtime"
        "from validation_package_codecs.Json.runtime"
    rewriteFiles
        packageDirectory
        @"from validation_package_codecs\.validation_package_yaml"
        "from validation_package_codecs.Yaml.validation_package_yaml"

    let fableModules = Path.Combine(packageDirectory, "fable_modules")
    deleteMatchingDirectories fableModules "validation_package_model"
    deleteMatchingDirectories fableModules "validation_package_codecs"

let private writeVersionedJavaScriptManifest outputDirectory =
    let source = Path.Combine("src", "ARCExpect", "Javascript", "package.json")
    let target = Path.Combine(outputDirectory, "package.json")
    let content =
        Regex("\"version\"\\s*:\\s*\"[^\"]+\"")
            .Replace(
                File.ReadAllText source,
                $"\"version\": \"{ARCExpectPackageVersion}\"",
                1
            )

    let content =
        Regex("\"@nfdi4plants/validationpackage-model\"\\s*:\\s*\"[^\"]+\"")
            .Replace(content, $"\"@nfdi4plants/validationpackage-model\": \"{ValidationPackageModelNativeVersion}\"", 1)

    let content =
        Regex("\"@nfdi4plants/validationpackage-codecs\"\\s*:\\s*\"[^\"]+\"")
            .Replace(content, $"\"@nfdi4plants/validationpackage-codecs\": \"{ValidationPackageCodecsNativeVersion}\"", 1)

    File.WriteAllText(target, content)
    File.Copy(
        Path.Combine("src", "ARCExpect", "Javascript", "index.js"),
        Path.Combine(outputDirectory, "index.js"),
        true
    )
    File.Copy(
        Path.Combine("src", "ARCExpect", "Javascript", "TopLevelAPI.js"),
        Path.Combine(outputDirectory, "TopLevelAPI.js"),
        true
    )

let private pythonPackageVersion =
    toPythonPackageVersion ARCExpectPackageVersion

let private writePythonBuildProject outputDirectory =
    let packageDirectory = Path.Combine(outputDirectory, "arcexpect")
    File.Copy(
        Path.Combine("src", "ARCExpect", "Python", "__init__.py"),
        Path.Combine(packageDirectory, "__init__.py"),
        true
    )
    File.Copy(
        Path.Combine("src", "ARCExpect", "Python", "top_level_api.py"),
        Path.Combine(packageDirectory, "top_level_api.py"),
        true
    )

    let pyproject =
        $"""[project]
name = "arcexpect"
version = "{pythonPackageVersion}"
description = "Portable ARC validation package authoring, execution, result contracts, and output writers."
license = "MIT"
requires-python = ">=3.12"
dependencies = [
    "fable-library==5.11.0",
    "validationpackage-model=={ValidationPackageModelPythonVersion}",
    "validationpackage-codecs=={ValidationPackageCodecsPythonVersion}",
]

[project.urls]
Homepage = "https://github.com/nfdi4plants/arc-validate"
Repository = "https://github.com/nfdi4plants/arc-validate.git"

[build-system]
requires = ["hatchling"]
build-backend = "hatchling.build"

[tool.hatch.build.targets.wheel]
packages = ["arcexpect"]
exclude = [
    "**/*.fs",
    "**/*.fsproj",
    "**/*.fableproj",
    "**/obj/**",
    "**/pyproject.toml",
    "**/*.md",
]
"""

    File.WriteAllText(Path.Combine(outputDirectory, "pyproject.toml"), pyproject)

let private packARCExpectJavaScript () =
    let outputDirectory = Path.Combine(portableArtifactsDir, "arcexpect-package", "javascript")
    transpileARCExpect ARCExpectJavaScriptProject "javascript" outputDirectory
    externalizeARCExpectJavaScriptDependencies outputDirectory
    writeVersionedJavaScriptManifest outputDirectory
    runNpm [ "pack"; Path.GetFullPath(outputDirectory); "--pack-destination"; Path.GetFullPath(packageDir) ] "."

let private packARCExpectPython () =
    let outputDirectory = Path.Combine(portableArtifactsDir, "arcexpect-package", "python")
    let packageDirectory = Path.Combine(outputDirectory, "arcexpect")
    recreateDirectory outputDirectory
    runDotNetCommand "fable" $"{ARCExpectPythonProject} --lang python --outDir \"{packageDirectory}\" --noCache" "."
    removeFableModulesGitIgnore packageDirectory
    externalizeARCExpectPythonDependencies packageDirectory
    writePythonBuildProject outputDirectory
    runUv [ "build"; "--wheel"; "--out-dir"; Path.GetFullPath(packageDir); Path.GetFullPath(outputDirectory) ] "."

let packARCExpect =
    BuildTask.create
        "PackARCExpect"
        [ cleanPortablePackages; preparePortableToolchain; validateReleaseMetadata ] {
            packPortableProject CoreProject.ProjFile
            packARCExpectJavaScript ()
            packARCExpectPython ()
        }
