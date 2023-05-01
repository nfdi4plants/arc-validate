module ARCValidate

open Expecto


let outputPath = System.IO.Path.Combine(ArcPaths.inputPath, "arc-validate-results.xml")

[<EntryPoint>]
let main argv =
    let testRunSummary = performTest (testList "ARCTests" [ValidateArc.filesystem; ValidateArc.isaTests]) 
    Expecto.writeJUnitSummary outputPath testRunSummary
    //match testRunSummary with
    //| x when x.failed
    0