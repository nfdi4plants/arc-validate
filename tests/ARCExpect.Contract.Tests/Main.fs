module ARCExpect.Contract.Tests.Main

open Fable.Pyxpecto

#if !FABLE_COMPILER_JAVASCRIPT && !FABLE_COMPILER_TYPESCRIPT
let (!!) (value: 'a) = value
#endif

#if FABLE_COMPILER_JAVASCRIPT || FABLE_COMPILER_TYPESCRIPT
open Fable.Core.JsInterop
#endif

[<EntryPoint>]
let main _ =
    !!Pyxpecto.runTests [||] PortableContractTests.tests
