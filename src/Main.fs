module ARCValidate

open Expecto


//let outputPath = System.IO.Path.Combine(ArcPaths.inputPath, "arc-validate-result.xml")
let outputPath = "arc-validate-result.xml"
[<EntryPoint>]
let main argv =
    performTest (testList "ARCTests" [ValidateArc.filesystem; ValidateArc.isaTests]) |> Expecto.writeJUnitSummary outputPath
    0
