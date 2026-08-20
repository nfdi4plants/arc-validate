namespace ARCValidate.Configuration

open System
open ARCValidate.PackageManagement
open ValidationPackage.Codecs
open ValidationPackage.Model

/// The exact cached package and argument tokens prepared for one configured child run.
type PreparedConfiguredValidation =
    {
        Package: CachedValidationPackage
        PackageArguments: string array
        ConfigSha256: string
    }

/// Rechecks parent resolution intent against exact configuration bytes without consulting AVPR.
[<RequireQualifiedAccess>]
module ChildExecution =

    let private fail path message =
        raise (ConfigurationException($"{path}: {message}"))

    let private isStable (version: SemVer) =
        String.IsNullOrEmpty(version.PreRelease)
        && String.IsNullOrEmpty(version.BuildMetadata)

    let private sameIdentity first second =
        SemVer.compareIdentity(first, second) = 0

    let private parseExactVersion value =
        match SemVer.tryParse value with
        | Some version when SemVer.toString version = value -> version
        | _ ->
            fail
                "--package-version"
                $"must be one canonical full semantic version, but was '{value}'"

    let private isLowerHex character =
        (character >= '0' && character <= '9')
        || (character >= 'a' && character <= 'f')

    let private verifyDigest
        (loaded: LoadedValidationConfig)
        (expectedDigest: string option)
        =
        match expectedDigest with
        | None -> ()
        | Some expected ->
            if expected.Length <> 64 || not (expected |> Seq.forall isLowerHex) then
                fail
                    "--validation-config-sha256"
                    "must contain exactly 64 lowercase hexadecimal characters"

            if not (String.Equals(expected, loaded.Sha256, StringComparison.Ordinal)) then
                fail
                    loaded.Path
                    "SHA-256 does not match --validation-config-sha256; the configuration bytes changed"

    let private exactlyOne path name selections =
        match selections with
        | [| selection |] -> selection
        | [||] -> fail path $"does not select package '{name}'"
        | _ -> fail path $"selects package '{name}' more than once"

    let private ensureCanonicalIntent
        path
        (resolvedVersion: SemVer)
        (selection: ValidationPackageSelection)
        =
        let requested = selection.Version

        match selection.RollForward with
        | RollForwardPolicy.Disable ->
            if not (sameIdentity resolvedVersion requested) then
                fail
                    path
                    $"resolved version '{SemVer.toString resolvedVersion}' does not equal the exact requested version '{SemVer.toString requested}'"
        | RollForwardPolicy.LatestPatch
        | RollForwardPolicy.LatestMinor as policy ->
            if not (isStable requested) then
                fail path "rolling version requests must not contain prerelease or build metadata"

            if not (isStable resolvedVersion) then
                fail path "a rolling resolved version must be stable"

            let withinBand =
                match policy with
                | RollForwardPolicy.LatestPatch ->
                    resolvedVersion.Major = requested.Major
                    && resolvedVersion.Minor = requested.Minor
                | RollForwardPolicy.LatestMinor ->
                    resolvedVersion.Major = requested.Major
                | _ -> false

            if
                not withinBand
                || SemVer.comparePrecedence(resolvedVersion, requested) < 0
            then
                let policyName =
                    match policy with
                    | RollForwardPolicy.LatestPatch -> "latest_patch"
                    | RollForwardPolicy.LatestMinor -> "latest_minor"
                    | _ -> invalidOp "unexpected rolling policy"

                fail
                    path
                    $"resolved version '{SemVer.toString resolvedVersion}' is outside the requested '{policyName}' band beginning at '{SemVer.toString requested}'"
        | value -> fail path $"uses unsupported roll_forward value '{value}'"

    let private ensureLegacyIntent
        path
        (resolvedVersion: SemVer)
        (selection: LegacyValidationPackageSelection)
        =
        if selection.HasVersion then
            if not (sameIdentity resolvedVersion selection.Version) then
                fail
                    path
                    $"resolved version '{SemVer.toString resolvedVersion}' does not equal the exact legacy version '{SemVer.toString selection.Version}'"
        elif not (isStable resolvedVersion) then
            fail path "a name-only legacy selection requires a stable resolved version"

    let private selectAndRecheck
        (loaded: LoadedValidationConfig)
        packageName
        resolvedVersion
        =
        let path = $"{loaded.Path}: validation_packages['{packageName}']"

        if loaded.Decoded.IsLegacy then
            let selection =
                loaded.Decoded.Legacy.ValidationPackages
                |> Array.filter (fun selection -> selection.Name = packageName)
                |> exactlyOne path packageName

            ensureLegacyIntent path resolvedVersion selection
            ValidationPackageSelection.create(packageName, resolvedVersion)
        else
            let config =
                try ValidationPackagesConfig.validate loaded.Decoded.Canonical
                with :? ArgumentException as error -> fail loaded.Path error.Message

            let selection =
                config.ValidationPackages
                |> Array.filter (fun selection -> selection.Name = packageName)
                |> exactlyOne path packageName

            ensureCanonicalIntent path resolvedVersion selection
            selection

    let private requireExactCachedPackage
        packageName
        version
        (cache: PackageCache)
        =
        let versionText = SemVer.toString version

        let package =
            PackageCache.tryGetPackage packageName versionText cache
            |> Option.defaultWith (fun () ->
                fail
                    $"{packageName}@{versionText}"
                    "is not installed in the configured package cache"
            )

        if isNull (box package) || isNull (box package.Metadata) then
            fail
                $"{packageName}@{versionText}"
                "the configured cache entry has no package metadata"

        let cachedIdentity =
            ValidationPackageMetadata.tryGetIdentity package.Metadata
            |> Option.defaultWith (fun () ->
                fail
                    $"{packageName}@{versionText}"
                    "the configured cache entry has an invalid package identity"
            )

        if
            cachedIdentity.Name <> packageName
            || not (sameIdentity cachedIdentity.Version version)
        then
            fail
                $"{packageName}@{versionText}"
                "the cached package metadata does not match its cache identity"

        package

    /// Loads and hashes the explicit configuration, rechecks the supplied exact
    /// version, selects that exact cache entry, and materializes logical argv tokens.
    let prepare
        (configPath: string)
        (expectedDigest: string option)
        (packageName: string)
        (resolvedVersion: string)
        (cache: PackageCache)
        =
        if String.IsNullOrWhiteSpace(packageName) then
            fail "--package" "must be non-empty"

        if isNull (box cache) then
            nullArg "cache"

        let loaded = ValidationConfigFile.load configPath
        verifyDigest loaded expectedDigest

        let version = parseExactVersion resolvedVersion
        let selection = selectAndRecheck loaded packageName version
        let package = requireExactCachedPackage packageName version cache

        let configuredArguments =
            try
                let declarations =
                    if isNull package.Metadata.Inputs then
                        Array.empty
                    else
                        package.Metadata.Inputs

                ValidationPackageSelection.materializeArguments(
                    selection,
                    declarations
                )
            with :? ArgumentException as error ->
                fail
                    $"{loaded.Path}: validation_packages['{packageName}'].inputs"
                    error.Message

        {
            Package = package
            PackageArguments = configuredArguments
            ConfigSha256 = loaded.Sha256
        }
