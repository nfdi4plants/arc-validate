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

        let variableName = "ARC_VALIDATE_AVPR_URL"
        let previousRegistryUrl = Environment.GetEnvironmentVariable(variableName)

        try
            Environment.SetEnvironmentVariable(variableName, "https://avpr-dev.nfdi4plants.org")
            let result = runTool tool initialArgs
            f tool initialArgs result
        finally
            Environment.SetEnvironmentVariable(variableName, previousRegistryUrl)
