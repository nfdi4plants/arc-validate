module ARCValidate

open Expecto

[<EntryPoint>]
let main argv =
    printfn "%s" FilesystemStructure.Paths.inputPath
    Tests.runTestsInAssembly defaultConfig argv