module internal TestUtils

open System
open System.IO
open type System.Environment
open Fake.Core
open Common.TestUtils

open ReferenceObjects

module Fixtures =

    let withToolExecution (resetEnv:bool) (tool:string) (initialArgs: string []) (f: string -> string[] -> ProcessResult<ProcessOutput> -> unit) () =
        if resetEnv then resetConfigEnvironment()
        let result = runTool tool initialArgs
        f tool initialArgs result