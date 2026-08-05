namespace ARCExpect

open Fable.Core

type private PythonTraceback =
    abstract format_exception: exn -> string array

[<RequireQualifiedAccess>]
module internal TargetException =

    [<ImportAll("traceback")>]
    let private traceback: PythonTraceback =
        nativeOnly

    let stackTrace error =
        traceback.format_exception(error)
        |> String.concat ""
