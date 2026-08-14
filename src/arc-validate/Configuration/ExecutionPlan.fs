namespace ARCValidate.Configuration

open System
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.Json
open ValidationPackage.Model

/// Encodes and strictly decodes the CLI-owned validation-plan JSON contract.
[<RequireQualifiedAccess>]
module ExecutionPlanCodec =

    [<Literal>]
    let SchemaUri = "https://nfdi4plants.github.io/arc-validate/schemas/v1/validation_plan.schema.json"

    let private fail path message =
        raise (ConfigurationException($"{path}: {message}"))

    let private policyString policy =
        match policy with
        | ExecutionPlanRollForward.Disable -> "disable"
        | ExecutionPlanRollForward.LatestPatch -> "latest_patch"
        | ExecutionPlanRollForward.LatestMinor -> "latest_minor"
        | ExecutionPlanRollForward.LegacyLatestStable -> "legacy_latest_stable"

    let private parsePolicy path value =
        match value with
        | "disable" -> ExecutionPlanRollForward.Disable
        | "latest_patch" -> ExecutionPlanRollForward.LatestPatch
        | "latest_minor" -> ExecutionPlanRollForward.LatestMinor
        | "legacy_latest_stable" -> ExecutionPlanRollForward.LegacyLatestStable
        | _ -> fail path $"unsupported roll_forward value '{value}'"

    let private parseVersion path value =
        match SemVer.tryParse value with
        | Some version when SemVer.toString version = value -> version
        | _ -> fail path $"'{value}' is not a canonical full semantic version"

    let private isStable (version: SemVer) =
        String.IsNullOrEmpty(version.PreRelease)
        && String.IsNullOrEmpty(version.BuildMetadata)

    let private validateHash path (value: string) =
        let isLowerHex character =
            (character >= '0' && character <= '9')
            || (character >= 'a' && character <= 'f')

        if isNull value || value.Length <> 64 || not (value |> Seq.forall isLowerHex) then
            fail path "must be a lowercase 64-character SHA-256 digest"

    let private validateSelection index (selection: ExecutionPlanSelection) =
        let path = $"validation_packages[{index}]"

        if String.IsNullOrWhiteSpace(selection.Name) then
            fail $"{path}.name" "must be non-empty"

        let requested =
            selection.RequestedVersion
            |> Option.map (parseVersion $"{path}.requested_version")

        let resolved = parseVersion $"{path}.resolved_version" selection.ResolvedVersion

        match selection.RollForward, requested with
        | ExecutionPlanRollForward.LegacyLatestStable, None ->
            if not (isStable resolved) then
                fail $"{path}.resolved_version" "legacy resolution must select a stable version"
        | ExecutionPlanRollForward.LegacyLatestStable, Some _ ->
            fail $"{path}.requested_version" "must be null for legacy_latest_stable"
        | ExecutionPlanRollForward.Disable, Some floor ->
            if SemVer.compareIdentity(floor, resolved) <> 0 then
                fail $"{path}.resolved_version" "must equal requested_version when roll_forward is disable"
        | ExecutionPlanRollForward.LatestPatch, Some floor ->
            if not (isStable floor && isStable resolved) then
                fail path "latest_patch requires stable requested and resolved versions"

            if
                floor.Major <> resolved.Major
                || floor.Minor <> resolved.Minor
                || SemVer.comparePrecedence(resolved, floor) < 0
            then
                fail $"{path}.resolved_version" "does not satisfy the latest_patch floor"
        | ExecutionPlanRollForward.LatestMinor, Some floor ->
            if not (isStable floor && isStable resolved) then
                fail path "latest_minor requires stable requested and resolved versions"

            if
                floor.Major <> resolved.Major
                || SemVer.comparePrecedence(resolved, floor) < 0
            then
                fail $"{path}.resolved_version" "does not satisfy the latest_minor floor"
        | _, None ->
            fail $"{path}.requested_version" "must be a semantic version for canonical roll-forward policies"

    /// Validates schema-independent and resolver-semantic execution-plan invariants.
    let validate plan =
        if isNull (box plan) then
            nullArg "plan"

        validateHash "config_sha256" plan.ConfigSha256

        plan.ArcSpecification
        |> Option.iter (fun version -> parseVersion "arc_specification" version |> ignore)

        if isNull plan.ValidationPackages then
            fail "validation_packages" "must be an array"

        plan.ValidationPackages |> Array.iteri validateSelection

        plan.ValidationPackages
        |> Array.countBy _.Name
        |> Array.tryFind (fun (_, count) -> count > 1)
        |> Option.iter (fun (name, _) ->
            fail "validation_packages" $"package name '{name}' is duplicated"
        )

        plan

    /// Creates a plan from preflighted resolution results.
    let create configSha256 (resolved: ResolvedValidationConfig) =
        {
            ConfigSha256 = configSha256
            ArcSpecification = resolved.ArcSpecification |> Option.map SemVer.toString
            ValidationPackages =
                resolved.ValidationPackages
                |> Array.map (fun selection ->
                    {
                        Name = selection.Name
                        RequestedVersion = selection.RequestedVersion |> Option.map SemVer.toString
                        RollForward = selection.RollForward
                        ResolvedVersion = SemVer.toString selection.ResolvedVersion
                    }
                )
        }
        |> validate

    /// Encodes one validated plan as deterministic UTF-8 JSON ending in LF.
    let encode plan =
        let plan = validate plan
        use stream = new MemoryStream()

        let options =
            JsonWriterOptions(
                Indented = true,
                NewLine = "\n",
                SkipValidation = false
            )

        use writer = new Utf8JsonWriter(stream, options)
        writer.WriteStartObject()
        writer.WriteString("$schema", SchemaUri)
        writer.WriteString("config_sha256", plan.ConfigSha256)

        plan.ArcSpecification
        |> Option.iter (fun value -> writer.WriteString("arc_specification", value))

        writer.WriteStartArray("validation_packages")

        plan.ValidationPackages
        |> Array.iter (fun selection ->
            writer.WriteStartObject()
            writer.WriteString("name", selection.Name)

            match selection.RequestedVersion with
            | Some version -> writer.WriteString("requested_version", version)
            | None -> writer.WriteNull("requested_version")

            writer.WriteString("roll_forward", policyString selection.RollForward)
            writer.WriteString("resolved_version", selection.ResolvedVersion)
            writer.WriteEndObject()
        )

        writer.WriteEndArray()
        writer.WriteEndObject()
        writer.Flush()
        stream.WriteByte(0x0Auy)
        stream.ToArray()

    let private ensureObject path (element: JsonElement) =
        if element.ValueKind <> JsonValueKind.Object then
            fail path "must be an object"

    let private ensureProperties path allowed (element: JsonElement) =
        ensureObject path element
        let properties = element.EnumerateObject() |> Seq.toArray

        properties
        |> Array.countBy _.Name
        |> Array.tryFind (fun (_, count) -> count > 1)
        |> Option.iter (fun (name, _) -> fail path $"property '{name}' is duplicated")

        properties
        |> Array.tryFind (fun property -> not (Set.contains property.Name allowed))
        |> Option.iter (fun property -> fail path $"property '{property.Name}' is not allowed")

    let private requiredProperty path (name: string) (element: JsonElement) =
        let mutable value = Unchecked.defaultof<JsonElement>

        if element.TryGetProperty(name, &value) then
            value
        else
            fail path $"required property '{name}' is missing"

    let private optionalProperty (name: string) (element: JsonElement) =
        let mutable value = Unchecked.defaultof<JsonElement>

        if element.TryGetProperty(name, &value) then Some value else None

    let private stringValue path (element: JsonElement) =
        if element.ValueKind <> JsonValueKind.String then
            fail path "must be a string"

        let value = element.GetString()
        if isNull value then fail path "must be a string" else value

    let private decodeSelection index (element: JsonElement) =
        let path = $"validation_packages[{index}]"

        ensureProperties
            path
            (Set.ofList [ "name"; "requested_version"; "roll_forward"; "resolved_version" ])
            element

        let requestedElement = requiredProperty path "requested_version" element

        let requested =
            match requestedElement.ValueKind with
            | JsonValueKind.Null -> None
            | JsonValueKind.String -> Some(stringValue $"{path}.requested_version" requestedElement)
            | _ -> fail $"{path}.requested_version" "must be a semantic-version string or null"

        {
            Name = requiredProperty path "name" element |> stringValue $"{path}.name"
            RequestedVersion = requested
            RollForward =
                requiredProperty path "roll_forward" element
                |> stringValue $"{path}.roll_forward"
                |> parsePolicy $"{path}.roll_forward"
            ResolvedVersion =
                requiredProperty path "resolved_version" element
                |> stringValue $"{path}.resolved_version"
        }

    /// Strictly decodes a locally allowlisted plan schema without network access.
    let decode (json: ReadOnlyMemory<byte>) =
        try
            use document = JsonDocument.Parse(json)
            let root = document.RootElement

            ensureProperties
                "$"
                (Set.ofList [ "$schema"; "config_sha256"; "arc_specification"; "validation_packages" ])
                root

            let schema = requiredProperty "$" "$schema" root |> stringValue "$.$schema"

            if schema <> SchemaUri then
                fail "$.$schema" $"unsupported execution-plan schema URI '{schema}'"

            let packagesElement = requiredProperty "$" "validation_packages" root

            if packagesElement.ValueKind <> JsonValueKind.Array then
                fail "$.validation_packages" "must be an array"

            {
                ConfigSha256 =
                    requiredProperty "$" "config_sha256" root
                    |> stringValue "$.config_sha256"
                ArcSpecification =
                    optionalProperty "arc_specification" root
                    |> Option.map (stringValue "$.arc_specification")
                ValidationPackages =
                    packagesElement.EnumerateArray()
                    |> Seq.mapi decodeSelection
                    |> Seq.toArray
            }
            |> validate
        with
        | :? JsonException as error ->
            raise (ConfigurationException($"validation_plan.json: invalid JSON: {error.Message}"))

    /// Verifies that a plan digest still matches the exact configuration bytes.
    let verifyConfigDigest (configBytes: byte array) plan =
        let actual =
            SHA256.HashData(configBytes)
            |> Convert.ToHexString
            |> _.ToLowerInvariant()

        if actual <> plan.ConfigSha256 then
            raise (ConfigurationException("validation_plan.json: config_sha256 does not match the exact configuration bytes"))

        plan

    /// Converts encoded plan bytes to a strict UTF-8 string for tests and diagnostics.
    let toUtf8String (bytes: byte array) = UTF8Encoding(false, true).GetString(bytes)
