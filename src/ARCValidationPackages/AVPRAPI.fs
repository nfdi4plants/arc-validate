namespace ARCValidationPackages

open AVPRClient
open ARCValidationPackages
open AVPRIndex
open System
open System.Net.Http

module AVPRAPI =

    module Errors =

        type ResponseNullError(msg : string) =
            inherit Exception(msg)

        type ServerSideError(msg : string) =
            inherit Exception(msg)

        type UnexpectedStatusCodeError(msg : string) =
            inherit Exception(msg)

        type DeserializationError(msg : string) =
            inherit Exception(msg)
            
        type GeneralError(msg : string) =
            inherit Exception(msg)

type AVPRAPI () =
    member private this.BaseUri = Uri("https://avpr.nfdi4plants.org")
    member private this.HttpClienHandler = new HttpClientHandler (UseCookies = false)
    member private this.HttpClient = new HttpClient(this.HttpClienHandler, true, BaseAddress=this.BaseUri)
    member this.Client = AVPRClient.Client(this.HttpClient)
    member this.GetAllPackages (): ValidationPackage [] = 
        try
            this.Client.GetAllPackagesAsync(System.Threading.CancellationToken.None)
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> Seq.toArray
        with
        | :? AVPRClient.ApiException as e -> 
            match e.Message with
            | m when m.ToLower().Contains "response was null" -> 
                raise (AVPRAPI.Errors.ResponseNullError(e.Message))
            | m when m.ToLower().Contains "http status code of the response was not expected" -> 
                raise (AVPRAPI.Errors.UnexpectedStatusCodeError(e.Message))
            | m when m.ToLower().Contains "server side error occurred" -> 
                raise (AVPRAPI.Errors.ServerSideError(e.Message))
            | m when m.ToLower().Contains "could not deserialize the response body string" -> 
                raise (AVPRAPI.Errors.DeserializationError(e.Message))
            | _ -> 
                raise (AVPRAPI.Errors.GeneralError(e.Message))
    member this.GetPackageByName (packageName: string): ValidationPackage =
        try
            this.Client.GetLatestPackageByNameAsync(packageName)
            |> Async.AwaitTask
            |> Async.RunSynchronously
        with
        | :? AVPRClient.ApiException as e -> 
            match e.Message with
            | m when m.ToLower().Contains "response was null" -> 
                raise (AVPRAPI.Errors.ResponseNullError(e.Message))
            | m when m.ToLower().Contains "http status code of the response was not expected" -> 
                raise (AVPRAPI.Errors.UnexpectedStatusCodeError(e.Message))
            | m when m.ToLower().Contains "server side error occurred" -> 
                raise (AVPRAPI.Errors.ServerSideError(e.Message))
            | m when m.ToLower().Contains "could not deserialize the response body string" -> 
                raise (AVPRAPI.Errors.DeserializationError(e.Message))
            | _ -> 
                raise (AVPRAPI.Errors.GeneralError(e.Message))
    member this.GetPackageByNameAndVersion (packageName: string) (version: string): ValidationPackage =
        try
            this.Client.GetPackageByNameAndVersionAsync(packageName, version)
            |> Async.AwaitTask
            |> Async.RunSynchronously
        with
        | :? AVPRClient.ApiException as e -> 
            match e.Message with
            | m when m.ToLower().Contains "response was null" -> 
                raise (AVPRAPI.Errors.ResponseNullError(e.Message))
            | m when m.ToLower().Contains "http status code of the response was not expected" -> 
                raise (AVPRAPI.Errors.UnexpectedStatusCodeError(e.Message))
            | m when m.ToLower().Contains "server side error occurred" -> 
                raise (AVPRAPI.Errors.ServerSideError(e.Message))
            | m when m.ToLower().Contains "could not deserialize the response body string" -> 
                raise (AVPRAPI.Errors.DeserializationError(e.Message))
            | _ -> 
                raise (AVPRAPI.Errors.GeneralError(e.Message))
        
    member this.downloadPackageScript (packageName: string, ?version: string) =
        let vp = 
            match version with
            | Some v ->  this.GetPackageByNameAndVersion packageName v
            | None -> this.GetPackageByName packageName
        let script =
            vp.PackageContent
            |> Text.Encoding.UTF8.GetString
        script