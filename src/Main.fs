module arc_validate
open Expecto

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
