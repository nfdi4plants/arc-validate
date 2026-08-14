module DocumentationTasks

open BlackFox.Fake
open System
open System.IO

open BasicTasks
open Helpers
open ProjectInfo

let private fsdocsRoot = "/arc-validate/fsdocs/"

let private buildARCExpectForDocs =
    BuildTask.create "BuildARCExpectForDocs" [] {
        runDotNetCommand
            "build"
            $"{CoreProject.ProjFile} --configuration {configuration} /p:warnon=3390"
            "."
    }

let private buildSite version =
    printfn "building docs with ARCExpect version %s" version
    Environment.SetEnvironmentVariable("ARC_VALIDATE_DOCS_VERSION", version)
    runUv [ "run"; "--group"; "docs"; "mkdocs"; "build"; "--strict" ] "."
    let schemaOutputDirectory = Path.Combine("site", "schemas", "v1")
    Directory.CreateDirectory(schemaOutputDirectory) |> ignore
    File.Copy(
        Path.Combine("schemas", "validation_plan.schema.json"),
        Path.Combine(schemaOutputDirectory, "validation_plan.schema.json"),
        true
    )
    runDotNet
        (sprintf
            "fsdocs build --eval --clean --input docs/fsdocs --output site/fsdocs --projects %s --properties Configuration=%s --parameters fsdocs-package-version %s root %s"
            (Path.GetFullPath CoreProject.ProjFile)
            configuration
            version
            fsdocsRoot)
        "."

let buildDocs =
    BuildTask.create "BuildDocs" [ buildARCExpectForDocs ] {
        buildSite stableDocsVersionTag
    }

let watchDocs =
    BuildTask.create "WatchDocs" [] {
        runUv [ "run"; "--group"; "docs"; "mkdocs"; "serve" ] "."
    }

let watchApiDocs =
    BuildTask.create "WatchApiDocs" [ buildARCExpectForDocs ] {
        runDotNet
            (sprintf
                "fsdocs watch --eval --input docs/fsdocs --projects %s --properties Configuration=%s --parameters fsdocs-package-version %s root /"
                (Path.GetFullPath CoreProject.ProjFile)
                configuration
                stableDocsVersionTag)
            "."
    }
