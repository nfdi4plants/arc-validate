module internal TestUtils

open System
open System.IO
open type System.Environment
open ARCValidationPackages
open Common.TestUtils

module Expect =
    open Expecto

    let packageCacheEqual (actual:PackageCache) (expected:PackageCache) =
        let actualKeysSorted, expectedKeysSorted = (actual.Keys |> Seq.sort), (expected.Keys |> Seq.sort)
        let countIsEqual = actual.Count = expected.Count
        let KeysAreEqual = 
            try
                Seq.zip actualKeysSorted expectedKeysSorted
                |> Seq.forall (fun (a,b) -> a = b)
            with _ -> false

        let mutable innerKeysAreEqual = false

        if KeysAreEqual then 
            innerKeysAreEqual <- 
                [for key in actual.Keys -> 
                    actual[key], expected[key]
                ]
                |> Seq.forall(fun (v1, v2) ->
                    try
                        Seq.zip (v1.Keys |> Seq.sort) (v2.Keys |> Seq.sort)
                        |> Seq.forall (fun (a,b) -> a = b)
                    with _ -> false
                )

        let mutable zippedValuesActual: seq<(string*string*CachedValidationPackage)> = Seq.empty
        let mutable zippedValuesExpected: seq<(string*string*CachedValidationPackage)> = Seq.empty
        
        if innerKeysAreEqual && KeysAreEqual then 
            zippedValuesActual <- 
                [
                for key in actual.Keys do
                    for innerKey in actual[key].Keys do 
                        yield (key, innerKey, actual[key][innerKey])
                ]
            zippedValuesExpected <- 
                [
                for key in expected.Keys do
                    for innerKey in expected[key].Keys do 
                        yield (key, innerKey, expected[key][innerKey])
                ]


        Expect.isTrue countIsEqual $"Count was not equal.{System.Environment.NewLine}    expected: {expected.Count}{System.Environment.NewLine}    actual: {actual.Count}"
        Expect.sequenceEqual expectedKeysSorted actualKeysSorted $"Keys were not equal."
        Expect.sequenceEqual zippedValuesActual zippedValuesExpected "Values were not equal."

module Result =
    
    let okValue = function
        | Ok value -> value
        | Error error -> failwithf "%A" error

module Fixtures =

    let withFreshConfigAndCachePreview (token:string option) (f: Config * PackageCache -> unit) () =
        resetConfigEnvironment()
        let freshConfig, freshCache = API.GetSyncedConfigAndCache(false, ?Token = token) |> Result.okValue
        f (freshConfig, freshCache)

    let withFreshConfigAndCacheRelease (f: Config * PackageCache -> unit) () =
        resetConfigEnvironment()
        let freshConfig, freshCache = API.GetSyncedConfigAndCache(true) |> Result.okValue
        f (freshConfig, freshCache)

    //let saveAndCachePackage (token:string option) (package:Package) =
    //    let freshConfig, freshCache = API.GetSyncedConfigAndCache(?Token = token) |> Result.okValue
    //    let updatedCache = API.SaveAndCachePackage(freshConfig, freshCache, package) |> Result.okValue
    //    updatedCache