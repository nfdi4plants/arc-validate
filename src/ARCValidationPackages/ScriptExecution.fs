namespace ARCValidationPackages

open Fake.Core
open Fake.DotNet

type ScriptExecution =
    
    static member runScriptWithArgs (scriptPath: string) (args: string []) =
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

    static member runScript (scriptPath: string) =
        ScriptExecution.runScriptWithArgs scriptPath [||]

    static member runPackageScriptWithArgs (package: CachedValidationPackage) (args: string []) =
        ScriptExecution.runScriptWithArgs package.LocalPath args

    static member runPackageScript (package: CachedValidationPackage) =
        ScriptExecution.runPackageScriptWithArgs package [||]
