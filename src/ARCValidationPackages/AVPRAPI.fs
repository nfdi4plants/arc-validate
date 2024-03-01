namespace ARCValidationPackages

open AVPRClient
open ARCValidationPackages
open AVPRIndex
open System
open System.Net.Http

type AVPRAPI () =
    member private this.BaseUri = Uri("https://avpr.nfdi4plants.org")
    member private this.HttpClienHandler = new HttpClientHandler (UseCookies = false)
    member private this.HttpClient = new HttpClient(this.HttpClienHandler, true, BaseAddress=this.BaseUri)
    member this.Client = AVPRClient.Client(this.HttpClient)
    member this.GetAllPackages (): ValidationPackage [] = 
        this.Client.GetAllPackagesAsync(System.Threading.CancellationToken.None)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        |> Seq.toArray
    member this.GetPackageByName (packageName: string): ValidationPackage =
        this.Client.GetLatestPackageByNameAsync(packageName)
        |> Async.AwaitTask
        |> Async.RunSynchronously
    member this.GetPackageByNameAndVersion (packageName: string) (version: string): ValidationPackage =
        this.Client.GetPackageByNameAndVersionAsync(packageName, version)
        |> Async.AwaitTask
        |> Async.RunSynchronously
        
    //member this.downloadPackageScript (packageIndex: ValidationPackageIndex) =
        
    //    let validationPackage =
    //        this.GetPackageByNameAndVersion 
    //            packageIndex.Metadata.Name
    //            (ValidationPackageMetadata.getSemanticVersionString packageIndex.Metadata)
    //    Text.Encoding.UTF8.GetString validationPackage.PackageContent