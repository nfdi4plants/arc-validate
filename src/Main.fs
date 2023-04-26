module ARCValidate

open Expecto


let outputPath = System.IO.Path.Combine(ArcPaths.inputPath, "arc-validate-result.xml")

[<EntryPoint>]
let main argv =
    printfn "%s" ArcPaths.inputPath
    //Tests.runTestsInAssembly defaultConfig [|yield! argv; FilesystemStructure.Paths.inputPath|]
    //Tests.runTestsInAssembly defaultConfig argv
    performTest (testList "ARCTests" [ValidateArc.filesystem; ValidateArc.isaTests]) |> Expecto.writeJUnitSummary outputPath
    0