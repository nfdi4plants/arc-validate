module internal TestUtils

open System
open System.IO
open type System.Environment
open ARCValidationPackages
open Common.TestUtils

module Expect =
    open Expecto

    let packageCacheContainsPackage (packageName: string) (packageVersion:string) (packageCache:PackageCache) =
        Expect.isTrue (packageCache.ContainsKey(packageName)) "package cache did not contain any version of the package"
        Expect.isTrue (packageCache.[packageName].ContainsKey(packageVersion)) "package cache did not contain the specific version of the package"

    let packageCacheDoesNotContainPackage (packageName: string) (packageVersion:string) (packageCache:PackageCache) =
        Expect.isFalse (packageCache.ContainsKey(packageName)) "package cache did not contain any version of the package"
        Expect.isFalse (packageCache.[packageName].ContainsKey(packageVersion)) "package cache did not contain the specific version of the package"

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

    module AVPRClient =

        let authorEqual (actual:AVPRClient.Author) (expected:AVPRClient.Author) =
            Expect.equal actual.FullName expected.FullName "AVPRClient.Author Name not equal"
            Expect.equal actual.Email expected.Email "AVPRClient.Author Email not equal"
            Expect.equal actual.Affiliation expected.Affiliation "AVPRClient.Author Affiliation not equal"
            Expect.equal actual.AffiliationLink expected.AffiliationLink "AVPRClient.Author AffiliationLink not equal"
            
        let ontologyAnnotationTagEqual (actual:AVPRClient.OntologyAnnotation) (expected:AVPRClient.OntologyAnnotation) =
            Expect.equal actual.Name expected.Name "AVPRClient.OntologyAnnotation Name not equal"
            Expect.equal actual.TermAccessionNumber expected.TermAccessionNumber "AVPRClient.OntologyAnnotation TermAccessionNumber not equal"
            Expect.equal actual.TermSourceREF expected.TermSourceREF "AVPRClient.OntologyAnnotation TermSourceREF not equal"

        let validationPackageEqual (actual: AVPRClient.ValidationPackage) (expected:AVPRClient.ValidationPackage) =
            Expect.equal actual.Name expected.Name "AVPRClient.ValidationPackage Name not equal"
            Expect.equal actual.Summary expected.Summary "AVPRClient.ValidationPackage Summary not equal"
            Expect.equal actual.MajorVersion expected.MajorVersion "AVPRClient.ValidationPackage MajorVersion not equal"
            Expect.equal actual.MinorVersion expected.MinorVersion "AVPRClient.ValidationPackage MinorVersion not equal"
            Expect.equal actual.PatchVersion expected.PatchVersion "AVPRClient.ValidationPackage PatchVersion not equal"
            Expect.equal actual.ReleaseNotes expected.ReleaseNotes "AVPRClient.ValidationPackage ReleaseNotes not equal"
            Expect.equal actual.CQCHookEndpoint expected.CQCHookEndpoint "AVPRClient.ValidationPackage CQCHookEndpoint not equal"

            Seq.zip actual.Authors expected.Authors 
            |> Seq.iter (fun (actual, expected) -> authorEqual actual expected)

            Seq.zip actual.Tags expected.Tags
            |> Seq.iter (fun (actual, expected) -> ontologyAnnotationTagEqual actual expected)

            Expect.sequenceEqual expected.Tags expected.Tags "AVPRClient.ValidationPackage Tags not equal"
            Expect.sequenceEqual expected.PackageContent expected.PackageContent "AVPRClient.ValidationPackage PackageContent not equal"



module Result =
    
    let okValue = function
        | Ok value -> value
        | Error error -> failwithf "%A" error

module Fixtures =

    let withFreshConfigAndCaches (token:string option) (f: Config * PackageCache * PackageCache -> unit) () =
        resetConfigEnvironment()
        let freshConfig, freshAVPRCache, freshPreviewCache = API.Common.GetSyncedConfigAndCache(?Token = token) |> Result.okValue
        f (freshConfig, freshAVPRCache, freshPreviewCache)

    //let saveAndCachePackage (token:string option) (package:Package) =
    //    let freshConfig, freshCache = API.GetSyncedConfigAndCache(?Token = token) |> Result.okValue
    //    let updatedCache = API.SaveAndCachePackage(freshConfig, freshCache, package) |> Result.okValue
    //    updatedCache

module AVPR =
    
    let api = new AVPRAPI()