namespace ARCValidationPackages

open AVPRClient
open ARCValidationPackages
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
    member this.getPackageIndex(packageName: string, ?version: string) =
        let validationPackage =
            match version with
            | Some v -> this.GetPackageByNameAndVersion packageName v
            | None -> this.GetPackageByName packageName
        let validationPackageMetadata =
            ValidationPackageMetadata.create(
                validationPackage.Name,
                validationPackage.Description,
                validationPackage.MajorVersion,
                validationPackage.MinorVersion,
                validationPackage.PatchVersion,
                true,
                (
                    validationPackage.Authors
                    |> Array.ofSeq
                    |> Array.map (fun author ->
                        Author.create(
                            author.FullName,
                            author.Email,
                            author.Affiliation,
                            author.AffiliationLink
                        )
                    )
                ),
                validationPackage.Tags |> Array.ofSeq,
                validationPackage.ReleaseNotes
            )
        let validationPackageIndex =
            ValidationPackageIndex.create(
                validationPackage.ReleaseDate,
                validationPackageMetadata
            )
        validationPackageIndex
        
    member this.downloadPackageScript (packageIndex: ValidationPackageIndex) =
        let validationPackage =
            this.GetPackageByNameAndVersion 
                packageIndex.Metadata.Name
                (ValidationPackageMetadata.getSemanticVersionString packageIndex.Metadata)
        Text.Encoding.UTF8.GetString validationPackage.PackageContent
    