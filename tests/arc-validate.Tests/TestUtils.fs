module internal TestUtils

open System
open System.IO
open type System.Environment
open Fake.Core
open Common.TestUtils

open ReferenceObjects

module Fixtures =

    let withToolExecution (tool:string) (args: string []) (token: string option) (f: string -> string[] -> ProcessResult<ProcessOutput> -> unit) () =
        resetConfigEnvironment()
        let args = 
            match token with
            | Some t -> Array.append [| "-t"; t |] args
            | None -> args
        let result = runTool tool args
        f tool args result