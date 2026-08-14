let [<Literal>]PACKAGE_METADATA = """(*
---
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: payload-validation
Summary: A validation package with calculated output data
Description: Shows how package computations become validation-summary payload JSON.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
---
*)"""

#r "nuget: ARCExpect, 7.0.0-preview.4"

open System.IO
open ARCExpect
open Fable.Pyxpecto

let metadata = Setup.Metadata(PACKAGE_METADATA)

let arguments = PackageArguments.fromCommandLine(metadata)

let filesChecked =
    Directory.EnumerateFiles(
        arguments.ArcDirectory,
        "*",
        SearchOption.AllDirectories
    )
    |> Seq.length

let validationPackage =
    Setup.ValidationPackage(
        metadata,
        CriticalValidationCases = [|
            testCase "the ARC contains files" <| fun () ->
                Expect.isTrue (filesChecked > 0) "files checked"
        |]
    )

let payload =
    dict [
        "Metrics", box (dict [
            ("FilesChecked", box filesChecked)
        ])
        "Package", box metadata.Name
    ]

validationPackage
|> Execute.ValidationPipeline(arguments, Payload = payload)

printfn $"Recorded payload metrics for {filesChecked} files."
