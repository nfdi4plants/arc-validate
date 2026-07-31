module Helpers

open BlackFox.Fake
open Fake.Core
open Fake.DotNet
open System
open System.IO
open System.Security

let initializeContext () =
    let execContext = Context.FakeExecutionContext.Create false "build.fsx" [ ]
    Context.setExecutionContext (Context.RuntimeContext.Fake execContext)

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

let runUv args workingDirectory =
    let cacheDirectory = Path.Combine("artifacts", "uv-cache") |> Path.GetFullPath
    ensureDirectory cacheDirectory
    runCommand "uv" ([ "--cache-dir"; cacheDirectory ] @ args) workingDirectory

let writeNuGetConfig path localPackageSource =
    let fullPath = resolveRepositoryPath path
    ensureDirectory (Path.GetDirectoryName fullPath)

    let localSource = SecurityElement.Escape(Path.GetFullPath localPackageSource)
    let publicSource = SecurityElement.Escape "https://api.nuget.org/v3/index.json"

    let content =
        $"""<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="local" value="{localSource}" />
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
