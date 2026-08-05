namespace ARCExpect

open System
open Fable.Pyxpecto

type TestID =
    | Guid
    | Name of string

type TestCaseBuilderSp(id: TestID) =
    member _.TryFinally(body, compensation) =
        try
            body()
        finally
            compensation()

    member _.TryWith(body, catchHandler) =
        try
            body()
        with error ->
            catchHandler error

    member _.Using(disposable: #IDisposable, body) =
        using disposable body

    member _.For(sequence, body) =
        for item in sequence do
            body item

    member _.While(condition, body) =
        while condition() do
            body()

    member _.Combine(first, second) =
        second()
        first

    member _.Zero() = ()
    member _.Delay body = body

    member _.Run body =
        match id with
        | Guid -> testCase (System.Guid.NewGuid().ToString()) body
        | Name name -> testCase name body

[<AutoOpen>]
module ARCExpect =

    /// Creates an ARC validation case using the shared Pyxpecto test model.
    let validationCase id =
        TestCaseBuilderSp(id)

    /// Passes if either supplied validation action succeeds.
    let either first second =
        try
            first()
        with firstError ->
            try
                second()
            with secondError ->
                Fable.Pyxpecto.Suspect.fail(
                    $"{firstError.Message} or {secondError.Message}"
                )
