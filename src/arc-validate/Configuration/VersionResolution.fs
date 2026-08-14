namespace ARCValidate.Configuration

open System
open ValidationPackage.Codecs
open ValidationPackage.Model

/// Implements deterministic validation-package version policy resolution.
[<RequireQualifiedAccess>]
module VersionResolution =

    let private fail path message =
        raise (ConfigurationException($"{path}: {message}"))

    let private isStable (version: SemVer) =
        String.IsNullOrEmpty(version.PreRelease)
        && String.IsNullOrEmpty(version.BuildMetadata)

    let private highestVersion versions =
        versions
        |> Array.sortWith (fun first second -> SemVer.compareIdentity(second, first))
        |> Array.tryHead

    let private versionsFor name (identities: ValidationPackageIdentity array) =
        identities
        |> Array.choose (fun identity ->
            if identity.Name = name then Some identity.Version else None
        )

    let private resolveExact path name requested identities =
        versionsFor name identities
        |> Array.tryFind (fun version -> SemVer.compareIdentity(version, requested) = 0)
        |> Option.defaultWith (fun () ->
            fail path $"exact package identity '{name}@{SemVer.toString requested}' is not published"
        )

    let private resolveRolling path name requested policy identities =
        if not (isStable requested) then
            fail
                path
                $"requested version '{SemVer.toString requested}' may use prerelease or build metadata only when roll_forward is disable"

        versionsFor name identities
        |> Array.filter isStable
        |> Array.filter (fun candidate ->
            let withinPolicy =
                match policy with
                | ExecutionPlanRollForward.LatestPatch ->
                    candidate.Major = requested.Major
                    && candidate.Minor = requested.Minor
                | ExecutionPlanRollForward.LatestMinor ->
                    candidate.Major = requested.Major
                | _ -> false

            withinPolicy
            && SemVer.comparePrecedence(candidate, requested) >= 0
        )
        |> highestVersion
        |> Option.defaultWith (fun () ->
            let policyName =
                match policy with
                | ExecutionPlanRollForward.LatestPatch -> "latest_patch"
                | ExecutionPlanRollForward.LatestMinor -> "latest_minor"
                | _ -> invalidOp "rolling resolution requires a rolling policy"

            fail
                path
                $"no stable package version is eligible for '{name}@{SemVer.toString requested}' with roll_forward '{policyName}'"
        )

    let private resolveCanonical index identities (selection: ValidationPackageSelection) =
        let path = $"validation_packages[{index}]"

        let planPolicy, resolved =
            match selection.RollForward with
            | RollForwardPolicy.Disable ->
                ExecutionPlanRollForward.Disable,
                resolveExact path selection.Name selection.Version identities
            | RollForwardPolicy.LatestPatch ->
                ExecutionPlanRollForward.LatestPatch,
                resolveRolling
                    path
                    selection.Name
                    selection.Version
                    ExecutionPlanRollForward.LatestPatch
                    identities
            | RollForwardPolicy.LatestMinor ->
                ExecutionPlanRollForward.LatestMinor,
                resolveRolling
                    path
                    selection.Name
                    selection.Version
                    ExecutionPlanRollForward.LatestMinor
                    identities
            | value -> fail path $"unsupported roll_forward value '{value}'"

        {
            Name = selection.Name
            RequestedVersion = Some selection.Version
            RollForward = planPolicy
            ResolvedVersion = resolved
            Selection = Some selection
        }

    let private resolveLegacy index identities (selection: LegacyValidationPackageSelection) =
        let path = $"validation_packages[{index}]"

        if selection.HasVersion then
            {
                Name = selection.Name
                RequestedVersion = Some selection.Version
                RollForward = ExecutionPlanRollForward.Disable
                ResolvedVersion = resolveExact path selection.Name selection.Version identities
                Selection = None
            }
        else
            let resolved =
                versionsFor selection.Name identities
                |> Array.filter isStable
                |> highestVersion
                |> Option.defaultWith (fun () ->
                    fail path $"no stable package version is published for '{selection.Name}'"
                )

            {
                Name = selection.Name
                RequestedVersion = None
                RollForward = ExecutionPlanRollForward.LegacyLatestStable
                ResolvedVersion = resolved
                Selection = None
            }

    /// Resolves all decoded selections locally against one authoritative package index.
    let resolve
        (decoded: DecodedValidationPackagesConfig)
        (identities: ValidationPackageIdentity array)
        =
        if isNull (box decoded) then
            nullArg "decoded"

        if isNull identities then
            nullArg "identities"

        if decoded.IsLegacy then
            let config = decoded.Legacy

            {
                ArcSpecification =
                    if config.HasArcSpecification then Some config.ArcSpecification else None
                ValidationPackages =
                    config.ValidationPackages
                    |> Array.mapi (fun index selection -> resolveLegacy index identities selection)
                IsLegacy = true
            }
        else
            let config = decoded.Canonical |> ValidationPackagesConfig.validate

            {
                ArcSpecification =
                    if config.HasArcSpecification then Some config.ArcSpecification else None
                ValidationPackages =
                    config.ValidationPackages
                    |> Array.mapi (fun index selection -> resolveCanonical index identities selection)
                IsLegacy = false
            }
