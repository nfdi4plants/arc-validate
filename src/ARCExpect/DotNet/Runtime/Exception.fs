namespace ARCExpect

[<RequireQualifiedAccess>]
module internal TargetException =

    let stackTrace (error: exn) =
        if isNull error.StackTrace then "" else error.StackTrace
