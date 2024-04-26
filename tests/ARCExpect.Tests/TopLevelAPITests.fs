module TopLevelAPITests

open ARCExpect
open AVPRIndex
open Expecto

[<Tests>]
let ``Toplevel API tests`` =
    testSequenced (testList "Toplevel API tests" [])