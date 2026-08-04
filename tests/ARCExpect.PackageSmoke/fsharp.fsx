#r "nuget: ARCExpect, 7.0.0-preview.2"

open ARCExpect
open ValidationPackage.Model

let metadata =
    ValidationPackageMetadata.create(
        "packed-fsi",
        "summary",
        "description",
        1,
        0,
        0,
        "FSharp",
        Inputs = [|
            CommandInputParameter.create(
                "echo",
                CommandInputType.create(CwlPrimitive.String, IsNullable = true),
                CommandInputBinding.create(Prefix = "--echo")
            )
        |]
    )

let arguments = PackageArguments.fromCommandLine(metadata)

if
    arguments.ArcDirectory <> "packed-arc"
    || arguments.OutputDirectory <> "packed-out"
    || arguments.TryGetString("echo") <> Some "literal; $(not-executed)"
then
    failwith "F# Interactive package arguments were not normalized correctly."
