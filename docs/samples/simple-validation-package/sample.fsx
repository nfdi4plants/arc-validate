let [<Literal>]PACKAGE_METADATA = """(*
---
$schema: "https://avpr.nfdi4plants.org/schemas/v1/validation-package-frontmatter.schema.json"
Name: simple-validation
Summary: A minimal ARCExpect package
Description: Shows YAML frontmatter, a critical test, and portable execution.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
---
*)"""

#r "nuget: ARCExpect, 7.0.0-preview.4"

open ARCExpect
open Fable.Pyxpecto

let metadata = Setup.Metadata(PACKAGE_METADATA)

let arguments = PackageArguments.fromCommandLine(metadata)

let validationPackage =
    Setup.ValidationPackage(
        metadata,
        CriticalValidationCases = [|
            testCase "the package has a stable name" <| fun () ->
                Expect.equal metadata.Name "simple-validation" "package name"
        |]
    )

validationPackage |> Execute.ValidationPipeline(arguments)

printfn $"Validation outputs written below {arguments.OutputDirectory}."
