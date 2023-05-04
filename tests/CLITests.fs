module CLITests

open Expecto
open Expecto.Tests
open Expecto.Impl
open Expecto.Logging
open System.IO
open System.Diagnostics

type CLIContext() =
    static member create 
        (
            ?p: string,
            ?o : string
        ) = 
            fun f ->
                let tool = $"arc-validate"
                let args = 
                    [
                        p |> Option.map (fun o -> $"-p {p} ") |> Option.defaultValue ""
                        o |> Option.map (fun o -> $"-o {o} ") |> Option.defaultValue ""
                    ]
                    |> String.concat ""

                let outFile = 
                    o 
                    |> Option.map (fun o -> Path.Combine(o, "arc-validate-results.xml"))
                    |> Option.defaultValue (Path.Combine(System.Environment.CurrentDirectory, "arc-validate-results.xml"))

                let procStartInfo = 
                    ProcessStartInfo(
                        UseShellExecute = false,
                        FileName = tool,
                        Arguments = args
                    )
                let proc = Process.Start(procStartInfo)
                let result = 
                    try 
                        proc.WaitForExit()
                        File.ReadAllText outFile
                    with e as _ -> 
                        $"{tool} {args} failed:{System.Environment.NewLine}{e.Message}"
                f result

[<Tests>]
let ``CLI Tests`` =
    testList "CLI tests" [
        testList "fixtures/test-arc" [
            yield! testFixture (CLIContext.create(p = "fixtures/test-arc")) [
                "arc-validate -p fixtures/arcs/test-arc", (fun xml -> fun () -> Expect.equal xml ReferenceObjects.IO.``test arc 1 validation results`` "xml test results created by cli command were incorrect")
            ]
        ]
    ]