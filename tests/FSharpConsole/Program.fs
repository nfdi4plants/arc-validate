open ARCValidationPackages
open ARCValidationPackages.API

let resultFS = FSharpScript.run @"W:\repos\nfdi4plants\arc-validate\tests\ARCValidationPackages.Tests\fixtures\testScript.fsx"
let resultPY = PythonScript.run @"W:\repos\nfdi4plants\arc-validate\tests\ARCValidationPackages.Tests\fixtures\testScript.py"

printfn "FSharpScript result: ExitCode=%d, Messages=%A" resultFS.ExitCode resultFS.Messages
printfn "Python result: ExitCode=%d, Messages=%A" resultPY.ExitCode resultPY.Messages

