module PackageTasks

open ProjectInfo

open MessagePrompts
open BasicTasks
open TestTasks

open BlackFox.Fake
open Fake.Core
open Fake.DotNet
open Fake.IO.Globbing.Operators
open Helpers
open System.IO
open System.Text.RegularExpressions


let pack = BuildTask.create "Pack" [ clean; build ] {
    [
        CoreProject
    ]
    |> List.iter (fun pInfo ->
        if promptYesNo $"creating stable package for {pInfo.Name}{System.Environment.NewLine}\tpackage version: {pInfo.PackageVersionTag}{System.Environment.NewLine}\tassembly version: {pInfo.AssemblyVersion}{System.Environment.NewLine}\tassembly informational version: {pInfo.AssemblyInformationalVersion}{System.Environment.NewLine} OK?" then
            pInfo.ProjFile
            |> Fake.DotNet.DotNet.pack (fun p ->
                let msBuildParams =
                    match pInfo.ReleaseNotes with
                    | Some r ->
                        { p.MSBuildParams with
                            Properties =
                                ([
                                    "Version",pInfo.PackageVersionTag
                                    "AssemblyVersion", pInfo.AssemblyVersion
                                    "AssemblyInformationalVersion", pInfo.AssemblyVersion
                                    "PackageReleaseNotes",  (r.Notes |> String.concat "\r\n")
                                    "TargetsForTfmSpecificContentInPackage", "" //https://github.com/dotnet/fsharp/issues/12320
                                    ]
                                    @ p.MSBuildParams.Properties)
                            DisableInternalBinLog = true
                        }
                    | _ ->
                        { p.MSBuildParams with
                            Properties =
                                ([
                                    "Version",pInfo.PackageVersionTag
                                    "AssemblyVersion", pInfo.AssemblyVersion
                                    "AssemblyInformationalVersion", pInfo.AssemblyVersion
                                    "TargetsForTfmSpecificContentInPackage", "" //https://github.com/dotnet/fsharp/issues/12320
                                    ]
                                    @ p.MSBuildParams.Properties)
                            DisableInternalBinLog = true
                        }
                        

                { p with
                    MSBuildParams = msBuildParams
                    OutputPath = Some pkgDir
                    NoBuild = true
                }
                |> DotNet.Options.withCustomParams (Some "-tl")
            )
        else
            failwith "aborted"
        )
    }

let packPrerelease =
    BuildTask.create
        "PackPrerelease"
        [
            clean
            build
        ] {
        [
            CoreProject
        ]
        |> List.iter (fun pInfo ->
            printfn $"Please enter pre-release package suffix for {pInfo.Name}"
            let prereleaseSuffix = System.Console.ReadLine()
            pInfo.PackagePrereleaseTag <- sprintf "%s-%s" pInfo.PackageVersionTag prereleaseSuffix
            if promptYesNo $"creating prerelease package for {pInfo.Name}{System.Environment.NewLine}\tpackage version: {pInfo.PackagePrereleaseTag}{System.Environment.NewLine}\tassembly version: {pInfo.AssemblyVersion}{System.Environment.NewLine}\tassembly informational version: {pInfo.AssemblyInformationalVersion}{System.Environment.NewLine} OK?" then
                pInfo.ProjFile
                |> Fake.DotNet.DotNet.pack (fun p ->
                    let msBuildParams =
                        match pInfo.ReleaseNotes with
                        | Some r ->
                            { p.MSBuildParams with
                                Properties =
                                    ([
                                        "Version",pInfo.PackagePrereleaseTag
                                        "AssemblyVersion", pInfo.AssemblyVersion
                                        "InformationalVersion", pInfo.AssemblyInformationalVersion
                                        "PackageReleaseNotes",  (r.Notes |> String.concat "\r\n")
                                        "TargetsForTfmSpecificContentInPackage", "" //https://github.com/dotnet/fsharp/issues/12320
                                        ])
                                DisableInternalBinLog = true
                            }
                        | _ -> 
                            { p.MSBuildParams with
                                Properties =
                                    ([
                                        "Version",pInfo.PackagePrereleaseTag
                                        "AssemblyVersion", pInfo.AssemblyVersion
                                        "InformationalVersion", pInfo.AssemblyInformationalVersion
                                        "TargetsForTfmSpecificContentInPackage", "" //https://github.com/dotnet/fsharp/issues/12320
                                        ])
                                DisableInternalBinLog = true
                            }

                    { p with
                        VersionSuffix = Some prereleaseSuffix
                        OutputPath = Some pkgDir
                        MSBuildParams = msBuildParams
                        NoBuild = true
                    }
                    |> DotNet.Options.withCustomParams (Some "-tl")
                )
            else
                failwith "aborted"
        )
    }

let cleanPortablePackages = BuildTask.create "CleanPortablePackages" [] {
    recreateDirectory packageDir
}

let private packPortableProject project =
    ensureDirectory packageDir

    project
    |> DotNet.pack (fun options ->
        { options with
            Configuration = DotNet.BuildConfiguration.Release
            OutputPath = Some packageDir
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
        "\"\\./fable_modules/ValidationPackage\\.Model\\.[^/]+/([^\"]+)\\.fs\\.js\""
        "\"validationpackage-model/$1.js\""
    rewriteFiles
        outputDirectory
        "\"\\./fable_modules/ValidationPackage\\.Codecs\\.[^/]+/([^\"]+)\\.fs\\.js\""
        "\"validationpackage-codecs/$1.js\""

    let fableModules = Path.Combine(outputDirectory, "fable_modules")
    deleteMatchingDirectories fableModules "ValidationPackage.Model.*"
    deleteMatchingDirectories fableModules "ValidationPackage.Codecs.*"

let private externalizeARCExpectPythonDependencies packageDirectory =
    rewriteFiles
        packageDirectory
        @"from \.fable_modules\.validation_package_model\."
        "from validation_package_model."
    rewriteFiles
        packageDirectory
        @"from \.fable_modules\.validation_package_codecs\."
        "from validation_package_codecs."

    let fableModules = Path.Combine(packageDirectory, "fable_modules")
    deleteMatchingDirectories fableModules "validation_package_model"
    deleteMatchingDirectories fableModules "validation_package_codecs"

let private writeVersionedJavaScriptManifest outputDirectory =
    let source = Path.Combine("src", "ARCExpect", "package.json")
    let target = Path.Combine(outputDirectory, "package.json")
    let content =
        Regex("\"version\"\\s*:\\s*\"[^\"]+\"")
            .Replace(
                File.ReadAllText source,
                $"\"version\": \"{ARCExpectPackageVersion}\"",
                1
            )

    let content =
        Regex("\"validationpackage-model\"\\s*:\\s*\"[^\"]+\"")
            .Replace(content, $"\"validationpackage-model\": \"{ValidationPackageModelNativeVersion}\"", 1)

    let content =
        Regex("\"validationpackage-codecs\"\\s*:\\s*\"[^\"]+\"")
            .Replace(content, $"\"validationpackage-codecs\": \"{ValidationPackageCodecsNativeVersion}\"", 1)

    File.WriteAllText(target, content)
    File.Copy(Path.Combine("src", "ARCExpect", "index.js"), Path.Combine(outputDirectory, "index.js"), true)

let private pythonPackageVersion =
    ARCExpectPackageVersion
        .Replace("-alpha.", "a")
        .Replace("-beta.", "b")
        .Replace("-rc.", "rc")

let private writePythonBuildProject outputDirectory =
    let packageDirectory = Path.Combine(outputDirectory, "arcexpect")
    File.Copy(Path.Combine("src", "ARCExpect", "__init__.py"), Path.Combine(packageDirectory, "__init__.py"), true)

    let pyproject =
        $"""[project]
name = "arcexpect"
version = "{pythonPackageVersion}"
description = "Portable ARC validation package authoring, execution, result contracts, and output writers."
license = "MIT"
requires-python = ">=3.12"
dependencies = [
    "fable-library==5.11.0",
    "validationpackage-model=={ValidationPackageModelNativeVersion}",
    "validationpackage-codecs=={ValidationPackageCodecsNativeVersion}",
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

let packARCExpect = BuildTask.create "PackARCExpect" [ cleanPortablePackages ] {
    packPortableProject CoreProject.ProjFile
    packARCExpectJavaScript ()
    packARCExpectPython ()
}
