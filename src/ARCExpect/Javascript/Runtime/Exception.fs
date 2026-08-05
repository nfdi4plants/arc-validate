namespace ARCExpect

open Fable.Core

[<RequireQualifiedAccess>]
module internal TargetException =

    [<Emit("$0?.stack ?? ''")>]
    let stackTrace (_error: exn): string =
        nativeOnly
