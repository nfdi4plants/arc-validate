namespace ARCExpect

open Fable.Core

type private PythonSystem =
    abstract argv: string array

[<RequireQualifiedAccess>]
module internal TargetCommandLine =

    [<ImportAll("sys")>]
    let private sys: PythonSystem = nativeOnly

    let arguments() = sys.argv[1..]
