module internal TestUtils

open System
open System.IO
open type System.Environment
open ARCValidationPackages
open Common.TestUtils

module Result =
    
    let okValue = function
        | Ok value -> value
        | Error error -> failwithf "%A" error

module Fixtures =

    let withFreshConfigAndCache (token:string option) (f: Config * PackageCache -> unit) () =
        resetConfigEnvironment()
        let freshConfig, freshCache = API.GetSyncedConfigAndCache(?Token = token) |> Result.okValue
        f (freshConfig, freshCache)

    //let saveAndCachePackage (token:string option) (package:Package) =
    //    let freshConfig, freshCache = API.GetSyncedConfigAndCache(?Token = token) |> Result.okValue
    //    let updatedCache = API.SaveAndCachePackage(freshConfig, freshCache, package) |> Result.okValue
    //    updatedCache