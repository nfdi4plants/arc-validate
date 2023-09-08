module LegacyArgs

open Argu
open System

[<Obsolete>]
type LegacyArguments =
    | [<AltCommandLine("-p")>] ARC_Directory of path:string
    | [<AltCommandLine("-o")>] Out_Directory of path:string
    | [<AltCommandLine("-v")>] Verbose

    interface IArgParserTemplate with
        member s.Usage =
            match s with
            | ARC_Directory _ -> "Optional. Specify a directory that contains the arc to convert. Default: content of the ARC_PATH environment variable. If ARC_PATH is not set: current directory."
            | Out_Directory _ -> "Optional. Specify a output directory for the test results file (arc-validate-results.xml). Default: file gets written to the arc root folder."
            | Verbose         -> "Use verbose error messages (with full error stack)."

[<Obsolete>]
let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some System.ConsoleColor.Red)

[<Obsolete>]
let legacyArgParser = ArgumentParser.Create<LegacyArguments>(programName = "arc-validate", errorHandler = errorHandler)