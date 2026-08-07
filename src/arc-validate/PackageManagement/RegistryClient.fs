namespace ARCValidate.PackageManagement

open System
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks
open AVPRClient

type RegistryError =
    | NotFound of message: string
    | RateLimitExceeded of message: string
    | ServerError of statusCode: int * message: string
    | UnexpectedStatus of statusCode: int * message: string
    | InvalidResponse of message: string
    | TransportError of message: string

exception RegistryRequestException of RegistryError

module private RegistryError =

    let ofApiException (error: ApiException) =
        match error.StatusCode with
        | 404 -> NotFound error.Message
        | 429 -> RateLimitExceeded error.Message
        | statusCode when statusCode >= 500 -> ServerError(statusCode, error.Message)
        | statusCode when statusCode >= 200 && statusCode < 300 -> InvalidResponse error.Message
        | statusCode -> UnexpectedStatus(statusCode, error.Message)

type RegistryClient(?BaseUri: Uri, ?HttpClient: HttpClient) =
    let baseUri =
        match BaseUri, HttpClient with
        | Some uri, _ -> uri
        | None, Some httpClient when not (isNull httpClient.BaseAddress) -> httpClient.BaseAddress
        | None, _ -> Uri(Defaults.REGISTRY_API_URL())

    let ownsHttpClient = HttpClient.IsNone

    let httpClient =
        match HttpClient with
        | Some httpClient -> httpClient
        | None ->
            let handler = new HttpClientHandler(UseCookies = false)
            new HttpClient(handler, true)

    do
        if isNull httpClient.BaseAddress then
            httpClient.BaseAddress <- baseUri

    let client =
        let generatedClient = AVPRClient.Client(httpClient)
        generatedClient.BaseUrl <- baseUri.ToString().TrimEnd('/') + "/"
        generatedClient

    let translateRequest (request: unit -> Task<'T>) =
        task {
            try
                return! request ()
            with
            | :? ApiException as error ->
                return raise (RegistryRequestException(RegistryError.ofApiException error))
            | :? HttpRequestException as error ->
                return raise (RegistryRequestException(TransportError error.Message))
            | :? TaskCanceledException as error ->
                return raise (RegistryRequestException(TransportError error.Message))
        }

    member _.BaseUri = baseUri

    member _.GetAllPackagesAsync(?cancellationToken: CancellationToken) =
        let cancellationToken = defaultArg cancellationToken System.Threading.CancellationToken.None

        translateRequest (fun () ->
            task {
                let! packages = client.GetAllPackagesAsync(cancellationToken)
                return packages |> Seq.toArray
            })

    member _.GetPackageByNameAsync(packageName: string, ?cancellationToken: CancellationToken) =
        let cancellationToken = defaultArg cancellationToken System.Threading.CancellationToken.None
        translateRequest (fun () -> client.GetLatestPackageByNameAsync(packageName, cancellationToken))

    member _.GetPackageByNameAndVersionAsync(
        packageName: string,
        version: string,
        ?cancellationToken: CancellationToken
    ) =
        let cancellationToken = defaultArg cancellationToken System.Threading.CancellationToken.None
        translateRequest (fun () -> client.GetPackageByNameAndVersionAsync(packageName, version, cancellationToken))

    member this.DownloadPackageScriptAsync(
        packageName: string,
        ?Version: string,
        ?cancellationToken: CancellationToken
    ) =
        task {
            let! package =
                match Version with
                | Some version ->
                    this.GetPackageByNameAndVersionAsync(
                        packageName,
                        version,
                        ?cancellationToken = cancellationToken
                    )
                | None -> this.GetPackageByNameAsync(packageName, ?cancellationToken = cancellationToken)

            return package.PackageContent |> Encoding.UTF8.GetString
        }

    interface IDisposable with
        member _.Dispose() =
            if ownsHttpClient then httpClient.Dispose()
