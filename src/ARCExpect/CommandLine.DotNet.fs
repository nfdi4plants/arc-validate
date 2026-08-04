namespace ARCExpect

open System

[<RequireQualifiedAccess>]
module internal TargetCommandLine =

    let arguments() =
        let arguments = Environment.GetCommandLineArgs()

        if
            arguments.Length >= 2
            && arguments[0].IndexOf("fsi", StringComparison.OrdinalIgnoreCase) >= 0
            && arguments[1].EndsWith(".fsx", StringComparison.OrdinalIgnoreCase)
        then
            arguments[2..]
        else
            arguments[1..]
