namespace ARCValidationPackages

open Fake.Core
open Fake.DotNet

module internal Tool =

    let run (tool: string) (args: string []) =
        CreateProcess.fromRawCommand tool args
        |> CreateProcess.redirectOutputIfNotRedirected
        |> fun p ->
        
            let results = System.Collections.Generic.List<ConsoleMessage>()

            let errorF msg = 
                Trace.traceError msg
                results.Add(ConsoleMessage.CreateError msg)

            let messageF msg =
                Trace.trace msg
                results.Add(ConsoleMessage.CreateOut msg)

            CreateProcess.withOutputEventsNotNull messageF errorF p
            |> CreateProcess.map (fun prev -> prev, (results |> List.ofSeq))
        |> CreateProcess.map (fun (r, results) -> ProcessResult.New r.ExitCode results)
        |> Proc.run

type FSharpScript =
    
    static member runWithArgs (scriptPath: string) (args: string []) =
        let args = Array.concat [|[|scriptPath|]; args|]
        DotNet.exec 
            (fun p -> 
                {
                    p with
                        RedirectOutput = true
                        PrintRedirectedOutput = true
                }
            )
            "fsi" 
            (args |> String.concat " ")

    static member run (scriptPath: string) =
        FSharpScript.runWithArgs scriptPath [||]

    static member runPackageScriptWithArgs (package: CachedValidationPackage) (args: string []) =
        FSharpScript.runWithArgs package.LocalPath args

    static member runPackageScript (package: CachedValidationPackage) =
        FSharpScript.runPackageScriptWithArgs package [||]


type PythonScript =

    static member runWithArgs (scriptPath: string) (args: string []) =
        let args = Array.concat [|[|"run"; scriptPath|]; args|]
        Tool.run "uv" args

    static member run (scriptPath: string) =
        PythonScript.runWithArgs scriptPath [||]

    static member runPackageScriptWithArgs (package: CachedValidationPackage) (args: string []) =
        PythonScript.runWithArgs package.LocalPath args

    static member runPackageScript (package: CachedValidationPackage) =
        PythonScript.runPackageScriptWithArgs package [||]