let [<Literal>]PACKAGE_METADATA = """(*
---
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: configurable-validation
Summary: A validation package with typed inputs
Description: Reads standard and CWL-defined arguments through ARCExpect.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
Inputs:
  - id: strict
    type: boolean
    inputBinding:
      prefix: --strict
  - id: minimum-files
    type: int
    inputBinding:
      prefix: --minimum-files
  - id: label
    type: string?
    inputBinding:
      prefix: --label
---
*)"""

#r "nuget: ARCExpect, 7.0.0-preview.4"

open ARCExpect
open Fable.Pyxpecto

let metadata = Setup.Metadata(PACKAGE_METADATA)

let arguments = PackageArguments.fromCommandLine(metadata)
let minimumFiles = arguments.GetInt("minimum-files")
let strict = arguments.GetBoolean("strict")
let label = arguments.TryGetString("label")

let validationPackage =
    Setup.ValidationPackage(
        metadata,
        CriticalValidationCases = [|
            testCase "minimum-files is positive" <| fun () ->
                Expect.isTrue (minimumFiles > 0) "minimum-files"

            testCase "strict mode has a label" <| fun () ->
                if strict then
                    Expect.isTrue label.IsSome "label"
        |]
    )

let payload =
    dict [
        "Strict", box strict
        "MinimumFiles", box minimumFiles

        match label with
        | Some value -> "Label", box value
        | None -> ()
    ]

validationPackage
|> Execute.ValidationPipeline(arguments, Payload = payload)

printfn $"Validated {arguments.ArcDirectory}; results belong in {arguments.OutputDirectory}."
