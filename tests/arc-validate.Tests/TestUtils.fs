module internal TestUtils

open System
open System.IO
open type System.Environment
open Fake.Core
open Common.TestUtils

open ReferenceObjects

module Fixtures =

    let withToolExecution (resetEnv:bool) (tool:string) (initialArgs: string []) (token: string option) (f: string -> string[] -> ProcessResult<ProcessOutput> -> unit) () =
        if resetEnv then resetConfigEnvironment()
        let args = 
            match token with
            | Some t -> Array.append [| "-t"; t |] initialArgs
            | None -> initialArgs
        let displayArgs = // let's not leak credentials in error message here
            match token with
                | Some t -> Array.append [| "-t"; "<API_TOKEN_WAS_SET>" |] initialArgs
                | None -> initialArgs
        let result = runTool tool args
        f tool displayArgs result