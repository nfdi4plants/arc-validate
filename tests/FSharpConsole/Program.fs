open ARCValidate.PackageRunner

let resultFS = FSharpScript.run "../arc-validate.Tests/fixtures/testScript.fsx"
let resultPY = PythonScript.run "../arc-validate.Tests/fixtures/testScript.py"

printfn $"F# exit code: {resultFS.ExitCode}"
printfn $"Python exit code: {resultPY.ExitCode}"
