module ARCValidate

open Expecto

[<EntryPoint>]
let main argv =
    printfn "%s" CheckFilesystemStructure.Paths.inputPath
    Tests.runTestsInAssembly defaultConfig argv