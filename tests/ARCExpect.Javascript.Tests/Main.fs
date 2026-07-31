module ARCExpect.Javascript.Tests.Main

open Fable.Core.JsInterop
open Fable.Pyxpecto

[<EntryPoint>]
let main _ =
    !!Pyxpecto.runTests [||] ARCExpect.Contract.Tests.PortableContractTests.tests
