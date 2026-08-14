namespace ARCValidate.API

open System
open System.IO
open System.Threading
open System.Threading.Tasks
open Argu
open ARCValidate
open ARCValidate.CLIArguments
open ARCValidate.Configuration
open ARCValidate.PackageManagement

/// Provides the configuration resolver command without mixing diagnostics into plan output.
[<RequireQualifiedAccess>]
module ConfigAPI =

    let private registryErrorMessage error =
        match error with
        | NotFound message -> $"registry discovery endpoint was not found: {message}"
        | RateLimitExceeded message -> $"registry rate limit exceeded: {message}"
        | ServerError(statusCode, message) -> $"registry server error ({statusCode}): {message}"
        | UnexpectedStatus(statusCode, message) -> $"registry request failed ({statusCode}): {message}"
        | InvalidResponse message -> $"registry returned an invalid response: {message}"
        | TransportError message -> $"registry could not be reached: {message}"

    /// Resolves a config using injected I/O and registry boundaries.
    let resolveAsync
        (configPath: string)
        (registry: IRegistryDiscoveryClient)
        (output: Stream)
        (diagnostics: TextWriter)
        (verbose: bool)
        (cancellationToken: CancellationToken)
        =
        task {
            try
                let loaded = ValidationConfigFile.load configPath

                if verbose then
                    do! diagnostics.WriteLineAsync($"Resolving validation configuration '{loaded.Path}'.")

                let! plan = ConfigurationResolver.resolveAsync registry loaded cancellationToken
                let encoded = ExecutionPlanCodec.encode plan

                if loaded.Decoded.IsLegacy then
                    do!
                        diagnostics.WriteLineAsync(
                            $"Warning: '{loaded.Path}' uses the legacy validation-packages format; add the canonical $schema, versions, and roll_forward fields."
                        )

                do! output.WriteAsync(encoded.AsMemory(), cancellationToken)
                do! output.FlushAsync(cancellationToken)
                return ExitCode.Success
            with
            | ConfigurationException message ->
                do! diagnostics.WriteLineAsync($"Configuration error: {message}")
                return ExitCode.ConfigurationError
            | RegistryRequestException error ->
                do! diagnostics.WriteLineAsync($"Registry error: {registryErrorMessage error}")
                return ExitCode.RegistryError
        }

    /// Executes the command using the configured registry and process standard streams.
    let resolve (args: ParseResults<ConfigResolveArgs>) verbose =
        let configPath = args.GetResult(ConfigResolveArgs.Validation_Config)

        try
            let config = Config.get()
            use registry = new RegistryClient(BaseUri = Uri(config.RegistryApiBaseUrl))

            resolveAsync
                configPath
                registry
                (Console.OpenStandardOutput())
                Console.Error
                verbose
                CancellationToken.None
            |> _.GetAwaiter().GetResult()
        with
        | :? UriFormatException as error ->
            Console.Error.WriteLine($"Configuration error: registry URL is invalid: {error.Message}")
            ExitCode.ConfigurationError
        | :? UnauthorizedAccessException as error ->
            Console.Error.WriteLine($"Configuration error: CLI settings cannot be read: {error.Message}")
            ExitCode.ConfigurationError
        | :? IOException as error ->
            Console.Error.WriteLine($"Configuration error: CLI settings cannot be read: {error.Message}")
            ExitCode.ConfigurationError
