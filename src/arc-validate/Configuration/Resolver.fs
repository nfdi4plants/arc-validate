namespace ARCValidate.Configuration

open System
open System.Threading
open System.Threading.Tasks
open ARCValidate.PackageManagement
open ValidationPackage.Model

/// Performs authoritative registry preflight before creating an execution plan.
[<RequireQualifiedAccess>]
module ConfigurationResolver =

    let private invalidRegistry message =
        raise (RegistryRequestException(InvalidResponse message))

    let private validateIndex (identities: ValidationPackageIdentity array) =
        if isNull identities then
            invalidRegistry "package-index response was null"

        identities
        |> Array.iteri (fun index identity ->
            if isNull (box identity) then
                invalidRegistry $"package-index identity at index {index} was null"

            if String.IsNullOrWhiteSpace(identity.Name) then
                invalidRegistry $"package-index identity at index {index} has an empty name"

            if isNull (box identity.Version) then
                invalidRegistry $"package-index identity at index {index} has no version"

            let canonical = SemVer.toString identity.Version

            match SemVer.tryParse canonical with
            | Some parsed when parsed = identity.Version -> ()
            | _ -> invalidRegistry $"package-index identity at index {index} has an invalid version"
        )

        identities
        |> Array.countBy (fun identity -> identity.Name, SemVer.toString identity.Version)
        |> Array.tryFind (fun (_, count) -> count > 1)
        |> Option.iter (fun ((name, version), _) ->
            invalidRegistry $"package-index contains duplicate identity '{name}@{version}'"
        )

        identities

    let private validateMetadata
        configPath
        index
        (resolution: ResolvedValidationPackage)
        (metadata: ValidationPackageMetadata)
        =
        let identity =
            if isNull (box metadata) then
                invalidRegistry $"metadata response for '{resolution.Name}' was null"

            match ValidationPackageMetadata.tryGetIdentity metadata with
            | Some identity -> identity
            | None -> invalidRegistry $"metadata response for '{resolution.Name}' has an invalid identity"

        if
            identity.Name <> resolution.Name
            || SemVer.compareIdentity(identity.Version, resolution.ResolvedVersion) <> 0
        then
            invalidRegistry
                $"metadata identity '{identity.Name}@{SemVer.toString identity.Version}' does not match requested identity '{resolution.Name}@{SemVer.toString resolution.ResolvedVersion}'"

        try CommandInputParameter.validate metadata.Inputs |> ignore
        with error ->
            invalidRegistry
                $"metadata declarations for '{resolution.Name}@{SemVer.toString resolution.ResolvedVersion}' are invalid: {error.Message}"

        let selection =
            match resolution.Selection with
            | Some selection -> selection
            | None ->
                ValidationPackageSelection.create(
                    resolution.Name,
                    resolution.ResolvedVersion
                )

        try ValidationPackageSelection.validateInputs(selection, metadata.Inputs)
        with
        | :? ArgumentException as error ->
            raise (
                ConfigurationException(
                    $"{configPath}: validation_packages[{index}] ('{resolution.Name}'): {error.Message}"
                )
            )

    /// Resolves versions from one index request and preflights exact metadata with at most four requests in flight.
    let resolveAsync
        (registry: IRegistryDiscoveryClient)
        (loaded: LoadedValidationConfig)
        (cancellationToken: CancellationToken)
        =
        task {
            if isNull (box registry) then
                nullArg "registry"

            let! identities = registry.GetPackageIndexAsync(cancellationToken)
            let identities = validateIndex identities
            let resolved = VersionResolution.resolve loaded.Decoded identities
            use gate = new SemaphoreSlim(4, 4)

            let preflight selectionIndex (selection: ResolvedValidationPackage) =
                task {
                    do! gate.WaitAsync(cancellationToken)

                    try
                        let version = SemVer.toString selection.ResolvedVersion

                        let! metadata =
                            registry.GetPackageMetadataAsync(
                                selection.Name,
                                version,
                                cancellationToken
                            )

                        validateMetadata loaded.Path selectionIndex selection metadata
                    finally
                        gate.Release() |> ignore
                }

            let! _ =
                resolved.ValidationPackages
                |> Array.mapi preflight
                |> Task.WhenAll

            return ExecutionPlanCodec.create loaded.Sha256 resolved
        }
