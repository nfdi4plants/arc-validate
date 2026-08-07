module Helpers

open BlackFox.Fake
open Fake.Core
open Fake.DotNet
open System
open System.IO
open System.Security
open System.Security.Cryptography

let initializeContext () =
    let execContext = Context.FakeExecutionContext.Create false "build.fsx" [ ]
    Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

let fileSha256 path =
    path
    |> File.ReadAllBytes
    |> SHA256.HashData
    |> Convert.ToHexString

/// Executes a dotnet command in the given working directory
let runDotNet cmd workingDir =
    let result =
        DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

let runCommand command args workingDirectory =
    let result =
        CreateProcess.fromRawCommand command args
        |> CreateProcess.withWorkingDirectory workingDirectory
        |> Proc.run

    if result.ExitCode <> 0 then
        failwithf "%s failed with exit code %i" command result.ExitCode

let runCommandWithEnvironmentVariable
    command
    args
    workingDirectory
    variableName
    variableValue
    =
    let result =
        CreateProcess.fromRawCommand command args
        |> CreateProcess.withWorkingDirectory workingDirectory
        |> CreateProcess.setEnvironmentVariable variableName variableValue
        |> Proc.run

    if result.ExitCode <> 0 then
        failwithf "%s failed with exit code %i" command result.ExitCode

let runDotNetCommand command arguments workingDirectory =
    let result =
        DotNet.exec
            (DotNet.Options.withWorkingDirectory workingDirectory)
            command
            arguments

    if not result.OK then
        failwithf "dotnet %s failed with exit code %i" command result.ExitCode

let private repositoryRoot = Path.GetFullPath "."

let private resolveRepositoryPath path =
    let fullPath = Path.GetFullPath path
    let rootPrefix = repositoryRoot.TrimEnd(Path.DirectorySeparatorChar) + string Path.DirectorySeparatorChar

    if not (fullPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase)) then
        failwithf "Refusing to modify a path outside the repository: %s" fullPath

    fullPath

let recreateDirectory path =
    let fullPath = resolveRepositoryPath path

    if Directory.Exists fullPath then
        Directory.Delete(fullPath, true)

    Directory.CreateDirectory fullPath |> ignore

let ensureDirectory path =
    path
    |> resolveRepositoryPath
    |> Directory.CreateDirectory
    |> ignore

let prepareFingerprintedNuGetSource destinationRoot primaryPackage packageDirectories =
    let fingerprint = fileSha256 primaryPackage
    let destination = Path.Combine(destinationRoot, fingerprint)
    ensureDirectory destination

    packageDirectories
    |> Seq.collect (fun directory -> Directory.EnumerateFiles(directory, "*.nupkg"))
    |> Seq.iter (fun package ->
        File.Copy(package, Path.Combine(destination, Path.GetFileName package), true)
    )

    destination

let runUv args workingDirectory =
    let cacheDirectory = Path.Combine("artifacts", "uv-cache") |> Path.GetFullPath
    ensureDirectory cacheDirectory
    runCommand "uv" ([ "--cache-dir"; cacheDirectory ] @ args) workingDirectory

let runNpm args workingDirectory =
    let cacheDirectory = Path.Combine("artifacts", "npm-cache") |> Path.GetFullPath
    ensureDirectory cacheDirectory
    let executable =
        if OperatingSystem.IsWindows() then
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                "nodejs",
                "npm.cmd"
            )
        else
            "npm"
    runCommand executable ([ "--cache"; cacheDirectory ] @ args) workingDirectory

let localNativeDependencyPackageDirectory () =
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

let localNativeDependencyArtifact directory fileName =
    let path = Path.Combine(directory, fileName)

    if not (File.Exists path) then
        failwithf
            "Required AVPR native package is missing: %s. Build AVPR PackPortablePackages first."
            path

    path

let writeNuGetConfig path localPackageSources =
    let fullPath = resolveRepositoryPath path
    ensureDirectory (Path.GetDirectoryName fullPath)

    let localSources =
        localPackageSources
        |> Seq.mapi (fun index source ->
            let escapedSource = SecurityElement.Escape(Path.GetFullPath source)
            $"    <add key=\"local-{index + 1}\" value=\"{escapedSource}\" />"
        )
        |> String.concat Environment.NewLine

    let publicSource = SecurityElement.Escape "https://api.nuget.org/v3/index.json"

    let content =
        $"""<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
{localSources}
    <add key="nuget.org" value="{publicSource}" protocolVersion="3" />
  </packageSources>
</configuration>
"""

    File.WriteAllText(fullPath, content)
    fullPath

let runOrDefault defaultTarget args =
    Trace.trace (sprintf "%A" args)
    try
        match args with
        | [| target |] -> Target.runOrDefault target
        | arr when args.Length > 1 ->
            Target.run 0 (Array.head arr) ( Array.tail arr |> List.ofArray )
        | _ -> BuildTask.runOrDefault defaultTarget
        0
    with e ->
        printfn "%A" e
        1
