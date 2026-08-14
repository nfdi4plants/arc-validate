module ConfigurationResolverTests

open System
open System.Collections.Generic
open System.IO
open System.Net
open System.Net.Http
open System.Text
open System.Threading
open System.Threading.Tasks
open Expecto
open ARCValidate
open ARCValidate.API
open ARCValidate.Configuration
open ARCValidate.PackageManagement
open ValidationPackage.Codecs
open ValidationPackage.Model

let private version value =
    SemVer.tryParse value |> Option.defaultWith (fun () -> failtestf "Invalid test SemVer: %s" value)

let private identity name value = ValidationPackageIdentity.create(name, version value)

let private metadata name value inputs =
    let semVer = version value

    ValidationPackageMetadata.create(
        name,
        "summary",
        "description",
        semVer.Major,
        semVer.Minor,
        semVer.Patch,
        "FSharp",
        Inputs = inputs
    )

type private StubDiscoveryClient(
    identities: ValidationPackageIdentity array,
    metadataFactory: string -> string -> ValidationPackageMetadata,
    ?delayMilliseconds: int
) =
    let delayMilliseconds = defaultArg delayMilliseconds 0
    let mutable indexCalls = 0
    let mutable metadataCalls = 0
    let mutable inFlight = 0
    let mutable maxInFlight = 0

    let updateMaximum current =
        let mutable observed = Volatile.Read(&maxInFlight)

        while current > observed && Interlocked.CompareExchange(&maxInFlight, current, observed) <> observed do
            observed <- Volatile.Read(&maxInFlight)

    member _.IndexCalls = indexCalls
    member _.MetadataCalls = metadataCalls
    member _.MaxInFlight = maxInFlight

    interface IRegistryDiscoveryClient with
        member _.GetPackageIndexAsync(_) =
            Interlocked.Increment(&indexCalls) |> ignore
            Task.FromResult(identities)

        member _.GetPackageMetadataAsync(name, requestedVersion, cancellationToken) =
            task {
                Interlocked.Increment(&metadataCalls) |> ignore
                let current = Interlocked.Increment(&inFlight)
                updateMaximum current

                try
                    if delayMilliseconds > 0 then
                        do! Task.Delay(delayMilliseconds, cancellationToken)

                    return metadataFactory name requestedVersion
                finally
                    Interlocked.Decrement(&inFlight) |> ignore
            }

type private FailingDiscoveryClient(error: RegistryError) =
    interface IRegistryDiscoveryClient with
        member _.GetPackageIndexAsync(_) =
            Task.FromException<ValidationPackageIdentity array>(RegistryRequestException error)

        member _.GetPackageMetadataAsync(_, _, _) =
            Task.FromException<ValidationPackageMetadata>(RegistryRequestException error)

type private DiscoveryHttpHandler() =
    inherit HttpMessageHandler()

    let requests = ResizeArray<string>()

    member _.Requests = requests |> Seq.toArray

    override _.SendAsync(request: HttpRequestMessage, _: CancellationToken) =
        let path = request.RequestUri.AbsolutePath
        requests.Add(path)

        let json =
            match path with
            | "/api/v1/package-index" ->
                """[{"Name":"package","Version":"1.0.0"}]"""
            | "/api/v1/packages/package/1.0.0/metadata" ->
                """{"Name":"package","Version":"1.0.0","Summary":"summary","Description":"description","ReleaseDate":"2026-01-01T00:00:00Z","Tags":[],"ReleaseNotes":"","CQCHookEndpoint":"","Authors":[],"ProgrammingLanguage":"FSharp","Inputs":[]}"""
            | _ -> failwith $"Unexpected discovery request: {path}"

        let response = new HttpResponseMessage(HttpStatusCode.OK)
        response.Content <- new StringContent(json, Encoding.UTF8, "application/json")
        Task.FromResult(response)

let private canonical selections =
    ValidationPackagesConfig.create(selections)
    |> DecodedValidationPackagesConfig.canonical

let private loaded decoded =
    {
        Path = "C:/arc/.arc/validation_packages.yml"
        Sha256 = String.replicate 64 "a"
        Decoded = decoded
    }

let private runResolver (registry: IRegistryDiscoveryClient) config =
    ConfigurationResolver.resolveAsync registry config CancellationToken.None
    |> _.GetAwaiter().GetResult()

let private withTemporaryDirectory action =
    let path = Path.Combine(Path.GetTempPath(), $"arc-validate-config-tests-{Guid.NewGuid():N}")
    Directory.CreateDirectory(path) |> ignore

    try action path
    finally Directory.Delete(path, true)

let private runConfigApi path (registry: IRegistryDiscoveryClient) =
    use output = new MemoryStream()
    use diagnostics = new StringWriter()

    let exitCode =
        ConfigAPI.resolveAsync
            path
            registry
            output
            diagnostics
            false
            CancellationToken.None
        |> _.GetAwaiter().GetResult()

    exitCode, output.ToArray(), diagnostics.ToString()

[<Tests>]
let ``configuration resolver tests`` =
    testList "configuration resolver" [
        test "one index request feeds ordered metadata preflight capped at four requests" {
            let selections =
                [| 0..6 |]
                |> Array.map (fun index ->
                    ValidationPackageSelection.create($"package-{index}", version "1.0.0")
                )

            let identities = selections |> Array.map (fun selection -> identity selection.Name "1.0.0")
            let stub = StubDiscoveryClient(identities, (fun name value -> metadata name value Array.empty), 30)

            let plan = runResolver stub (loaded (canonical selections))

            Expect.equal stub.IndexCalls 1 "Index request count"
            Expect.equal stub.MetadataCalls selections.Length "Exact metadata count"
            Expect.isGreaterThan stub.MaxInFlight 1 "Metadata requests ran concurrently"
            Expect.isLessThanOrEqual stub.MaxInFlight 4 "Metadata concurrency cap"
            Expect.sequenceEqual
                (plan.ValidationPackages |> Array.map _.Name)
                (selections |> Array.map _.Name)
                "Source order"
        }

        test "registry client uses only lightweight index and exact metadata endpoints" {
            use handler = new DiscoveryHttpHandler()
            use httpClient = new HttpClient(handler, false)
            let baseUri = Uri("https://registry.invalid")
            httpClient.BaseAddress <- baseUri
            use registry = new RegistryClient(BaseUri = baseUri, HttpClient = httpClient)
            let discovery = registry :> IRegistryDiscoveryClient

            let identities = discovery.GetPackageIndexAsync(CancellationToken.None).GetAwaiter().GetResult()
            let result =
                discovery.GetPackageMetadataAsync("package", "1.0.0", CancellationToken.None)
                    .GetAwaiter().GetResult()

            Expect.equal identities.Length 1 "Index mapping"
            Expect.equal result.Name "package" "Metadata mapping"
            Expect.sequenceEqual
                handler.Requests
                [|
                    "/api/v1/package-index"
                    "/api/v1/packages/package/1.0.0/metadata"
                |]
                "No heavy endpoint fallback"
        }

        test "metadata identity and declaration defects are registry errors" {
            let selection = ValidationPackageSelection.create("package", version "1.0.0")
            let stub = StubDiscoveryClient(
                [| identity "package" "1.0.0" |],
                (fun _ _ -> metadata "different" "1.0.0" Array.empty)
            )

            try
                runResolver stub (loaded (canonical [| selection |])) |> ignore
                failtest "Invalid metadata unexpectedly passed."
            with
            | RegistryRequestException(InvalidResponse _) -> ()
            | error -> failtestf "Expected invalid registry response, got %A" error
        }

        test "input mismatch completes preflight before emitting any JSON" {
            withTemporaryDirectory (fun root ->
                let path = Path.Combine(root, "validation_packages.yml")
                File.WriteAllText(
                    path,
                    """$schema: https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json
validation_packages:
  - name: package
    version: 1.0.0
    inputs:
      count: wrong
"""
                )

                let declaration =
                    CommandInputParameter.create(
                        "count",
                        CommandInputType.create(CwlPrimitive.Int),
                        CommandInputBinding.create(Prefix = "--count")
                    )

                let stub = StubDiscoveryClient(
                    [| identity "package" "1.0.0" |],
                    (fun name value -> metadata name value [| declaration |])
                )

                let exitCode, output, diagnostics = runConfigApi path stub
                Expect.equal exitCode ExitCode.ConfigurationError "Failure classification"
                Expect.isEmpty output "No partial plan"
                Expect.stringContains diagnostics "validation_packages[0]" "Package path"
                Expect.stringContains diagnostics "count" "Input path"
            )
        }

        test "strict UTF-8 BOM is included in the exact byte digest" {
            withTemporaryDirectory (fun root ->
                let path = Path.Combine(root, "validation_packages.yml")
                let yaml =
                    """$schema: https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json
validation_packages: []
"""

                let content = Encoding.UTF8.GetBytes(yaml)
                let bytes = Array.concat [| Encoding.UTF8.GetPreamble(); content |]
                File.WriteAllBytes(path, bytes)
                let stub = StubDiscoveryClient(Array.empty, (fun name value -> metadata name value Array.empty))
                let exitCode, output, diagnostics = runConfigApi path stub
                let plan = ExecutionPlanCodec.decode(ReadOnlyMemory<byte>(output))

                let expected =
                    System.Security.Cryptography.SHA256.HashData(bytes)
                    |> Convert.ToHexString
                    |> _.ToLowerInvariant()

                let withoutBom =
                    System.Security.Cryptography.SHA256.HashData(content)
                    |> Convert.ToHexString
                    |> _.ToLowerInvariant()

                Expect.equal exitCode ExitCode.Success "BOM config succeeds"
                Expect.equal plan.ConfigSha256 expected "Digest includes BOM"
                Expect.notEqual plan.ConfigSha256 withoutBom "Digest differs without BOM"
                Expect.equal diagnostics "" "Canonical success has no diagnostics"
            )
        }

        test "unsafe YAML and missing files return code 4 with no partial JSON" {
            withTemporaryDirectory (fun root ->
                let unsafePath = Path.Combine(root, "unsafe.yml")
                File.WriteAllText(
                    unsafePath,
                    """validation_packages:
  - &selection
    name: package
"""
                )

                let stub = StubDiscoveryClient(Array.empty, (fun name value -> metadata name value Array.empty))

                [ unsafePath; Path.Combine(root, "missing.yml") ]
                |> List.iter (fun path ->
                    let exitCode, output, diagnostics = runConfigApi path stub
                    Expect.equal exitCode ExitCode.ConfigurationError "Configuration exit code"
                    Expect.isEmpty output "No partial output"
                    Expect.stringContains diagnostics (Path.GetFileName(path)) "Path-rich diagnostic"
                )
            )
        }

        test "legacy warning appears once and registry failures use code 5" {
            withTemporaryDirectory (fun root ->
                let legacyPath = Path.Combine(root, "legacy.yml")
                File.WriteAllText(legacyPath, "validation_packages: []\n")
                let stub = StubDiscoveryClient(Array.empty, (fun name value -> metadata name value Array.empty))
                let exitCode, output, diagnostics = runConfigApi legacyPath stub

                Expect.equal exitCode ExitCode.Success "Legacy config succeeds"
                Expect.isGreaterThan output.Length 0 "Complete plan"
                Expect.equal
                    (diagnostics.Split("Warning:", StringSplitOptions.None).Length - 1)
                    1
                    "One migration warning"

                let canonicalPath = Path.Combine(root, "canonical.yml")
                File.WriteAllText(
                    canonicalPath,
                    """$schema: https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json
validation_packages: []
"""
                )

                let exitCode, failedOutput, failedDiagnostics =
                    runConfigApi canonicalPath (FailingDiscoveryClient(TransportError "offline"))

                Expect.equal exitCode ExitCode.RegistryError "Registry exit code"
                Expect.isEmpty failedOutput "No partial registry plan"
                Expect.stringContains failedDiagnostics "Registry error" "stderr classification"
            )
        }
    ]
