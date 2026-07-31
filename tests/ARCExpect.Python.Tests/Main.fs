module ARCExpect.Python.Tests.Main

open Fable.Pyxpecto

let (!!) (value: 'a) = value

[<EntryPoint>]
let main _ =
    !!Pyxpecto.runTests [||] ARCExpect.Contract.Tests.PortableContractTests.tests
