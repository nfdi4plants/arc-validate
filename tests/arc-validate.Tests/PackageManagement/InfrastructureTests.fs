module InfrastructureTests

open System
open System.IO
open System.Net
open System.Net.Http
open System.Threading
open System.Threading.Tasks
open Expecto
open ARCValidate.PackageManagement
open ARCValidate.PackageRunner

type private StubHttpMessageHandler(response: HttpResponseMessage) =
    inherit HttpMessageHandler()

    let mutable requestUri: Uri option = None

    member _.RequestUri = requestUri

    override _.SendAsync(request: HttpRequestMessage, _: CancellationToken) =
        requestUri <- Option.ofObj request.RequestUri
        Task.FromResult(response)

let private withTemporaryDirectory action =
    let path = Path.Combine(Path.GetTempPath(), $"arc-validate-package-tests-{Guid.NewGuid():N}")
    Directory.CreateDirectory(path) |> ignore

    try
        action path
    finally
        if Directory.Exists(path) then Directory.Delete(path, true)

[<Tests>]
let ``package infrastructure tests`` =
    testList "package infrastructure" [
        test "existing config controls the package cache folder" {
            withTemporaryDirectory (fun root ->
                let configPath = Path.Combine(root, "config", Defaults.CONFIG_FILE_NAME)
                let cacheFolder = Path.Combine(root, "custom package cache")

                Config.create(
                    packageCacheFolder = cacheFolder,
                    configFilePath = configPath,
                    registryApiBaseUrl = "https://avpr-dev.nfdi4plants.org"
                )
                |> Config.write()

                let config, _ =
                    Common.GetSyncedConfigAndCache(ConfigPath = configPath)
                    |> function
                        | Ok value -> value
                        | Error error -> failtestf "sync failed: %A" error

                Expect.equal config.PackageCacheFolder cacheFolder "configured cache folder was not retained"
                Expect.isTrue
                    (File.Exists(Path.Combine(cacheFolder, Defaults.PACKAGE_CACHE_FILE_NAME)))
                    "cache was not created in the configured folder"
            )
        }

        test "config and cache writes replace files without temporary-file residue" {
            withTemporaryDirectory (fun root ->
                let configPath = Path.Combine(root, "config.json")
                let cacheFolder = Path.Combine(root, "cache")
                let config = Config.create(cacheFolder, configPath)

                config |> Config.write()
                config |> Config.write()
                PackageCache() |> PackageCache.write(cacheFolder)
                PackageCache() |> PackageCache.write(cacheFolder)

                let temporaryFiles = Directory.EnumerateFiles(root, "*.tmp", SearchOption.AllDirectories)
                Expect.isEmpty temporaryFiles "atomic writes left temporary files behind"
                Expect.equal (Config.read(configPath)) config "replaced config was not readable"
                Expect.equal (PackageCache.read(Path.Combine(cacheFolder, Defaults.PACKAGE_CACHE_FILE_NAME))).Count 0 "replaced cache was not readable"
            )
        }

        test "legacy config without a registry endpoint remains readable" {
            withTemporaryDirectory (fun root ->
                let configPath = Path.Combine(root, "legacy-config.json")
                let cacheFolder = Path.Combine(root, "legacy-cache").Replace("\\", "/")
                let normalizedConfigPath = configPath.Replace("\\", "/")

                File.WriteAllText(
                    configPath,
                    $"""{{
  "PackageCacheFolder": "{cacheFolder}",
  "ConfigFilePath": "{normalizedConfigPath}"
}}"""
                )

                let config = Config.read(configPath)

                Expect.equal config.PackageCacheFolder cacheFolder "legacy cache folder changed"
                Expect.equal config.RegistryApiBaseUrl Defaults.REGISTRY_API_BASE_URL "default registry was not restored"
            )
        }

        test "registry errors are classified from HTTP status codes" {
            use response = new HttpResponseMessage(HttpStatusCode.NotFound)
            response.Content <- new StringContent("missing")
            use handler = new StubHttpMessageHandler(response)
            use httpClient = new HttpClient(handler, false)
            let baseUri = Uri("https://registry.invalid")
            httpClient.BaseAddress <- baseUri
            use registry = new RegistryClient(BaseUri = baseUri, HttpClient = httpClient)

            let actual =
                try
                    registry.GetPackageByNameAndVersionAsync("missing", "1.0.0").GetAwaiter().GetResult()
                    |> ignore
                    failtest "request unexpectedly succeeded"
                with
                | RegistryRequestException error -> error

            match actual with
            | NotFound _ -> ()
            | error -> failtestf "expected NotFound, got %A" error

            Expect.equal
                handler.RequestUri
                (Some(Uri("https://registry.invalid/api/v1/packages/missing/1.0.0")))
                "generated client request URI was not rooted below the registry base URI"
        }

        test "F# runner preserves arguments containing spaces" {
            let result = FSharpScript.runWithArgs "fixtures/testScriptArgs.fsx" [| "hello world" |]

            Expect.equal result.ExitCode 0 "F# fixture failed"
            Expect.sequenceEqual result.Messages [ "args: [|\"hello world\"|]" ] "F# argument was split"
        }

        test "Python runner preserves arguments containing spaces" {
            let result = PythonScript.runWithArgs "fixtures/testScriptArgs.py" [| "hello world" |]

            Expect.equal result.ExitCode 0 "Python fixture failed"
            Expect.sequenceEqual result.Messages [ "args: ['hello world']" ] "Python argument was split"
        }
    ]
