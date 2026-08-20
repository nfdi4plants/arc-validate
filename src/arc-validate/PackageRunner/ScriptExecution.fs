namespace ARCValidate.PackageRunner

open System
open System.Diagnostics
open System.Text
open ARCValidate.PackageManagement

type ProcessResult =
    {
        ExitCode: int
        Messages: string list
        Errors: string list
    }
    member this.OK = this.ExitCode = 0

module private Tool =

    let private lines (text: string) =
        text
            .ReplaceLineEndings("\n")
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
        |> Array.toList

    let run (tool: string) (arguments: string array) =
        let startInfo =
            ProcessStartInfo(
                FileName = tool,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            )

        arguments |> Array.iter startInfo.ArgumentList.Add

        use childProcess = new Process(StartInfo = startInfo)
        childProcess.Start() |> ignore

        let output = childProcess.StandardOutput.ReadToEndAsync()
        let error = childProcess.StandardError.ReadToEndAsync()

        childProcess.WaitForExit()

        let messages = output.GetAwaiter().GetResult() |> lines
        let errors = error.GetAwaiter().GetResult() |> lines

        messages |> List.iter Console.Out.WriteLine
        errors |> List.iter Console.Error.WriteLine

        {
            ExitCode = childProcess.ExitCode
            Messages = messages
            Errors = errors
        }

type FSharpScript =

    static member runWithArgs (scriptPath: string) (arguments: string array) =
        Array.concat [ [| "fsi"; scriptPath |]; arguments ]
        |> Tool.run "dotnet"

    static member run(scriptPath: string) = FSharpScript.runWithArgs scriptPath [||]

    static member runPackageScriptWithArgs (package: CachedValidationPackage) (arguments: string array) =
        FSharpScript.runWithArgs package.LocalPath arguments

    static member runPackageScript(package: CachedValidationPackage) =
        FSharpScript.runPackageScriptWithArgs package [||]

type PythonScript =

    static member runWithArgs (scriptPath: string) (arguments: string array) =
        Array.concat [ [| "run"; scriptPath |]; arguments ]
        |> Tool.run "uv"

    static member run(scriptPath: string) = PythonScript.runWithArgs scriptPath [||]

    static member runPackageScriptWithArgs (package: CachedValidationPackage) (arguments: string array) =
        PythonScript.runWithArgs package.LocalPath arguments

    static member runPackageScript(package: CachedValidationPackage) =
        PythonScript.runPackageScriptWithArgs package [||]
