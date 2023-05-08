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
                    |> Option.defaultValue (System.Environment.GetEnvironmentVariable("ARC_PATH")) // default to ARC_PATH if argument is not provided
                    |> fun s -> 
                        if p.IsSome then 
                            p.Value else 
                        if System.String.IsNullOrWhiteSpace(s) then 
                            System.Environment.CurrentDirectory 
                        else s // default to ./ if ARC_PATH is not set
                    |> fun s -> Path.Combine(s, "arc-validate-results.xml")

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
        testList "fixtures/arcs/inveniotestarc" [
            yield! testFixture (CLIContext.create(p = "fixtures/arcs/inveniotestarc")) [
                "arc-validate -p fixtures/arcs/inveniotestarc", (fun xml -> fun () -> Expect.equal xml ReferenceObjects.IO.``invenio test arc validation results`` "xml test results created by cli command were incorrect")
            ]
        ]
    ]