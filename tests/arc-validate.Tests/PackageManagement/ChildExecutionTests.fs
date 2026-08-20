module ChildExecutionTests

open System
open System.IO
open System.Text
open ARCValidate.CLIArguments
open ARCValidate.Configuration
open ARCValidate.PackageManagement
open ARCValidate.PackageRunner
open Expecto
open ValidationPackage.Model
open ReferenceObjects

let private schema =
    "https://avpr.nfdi4plants.org/schemas/v1/validation-packages.schema.json"

let private withTemporaryDirectory action =
    let path =
        Path.Combine(
            Path.GetTempPath(),
            $"arc-validate-child-execution-{Guid.NewGuid():N}"
        )

    Directory.CreateDirectory(path) |> ignore

    try action path
    finally
        if Directory.Exists(path) then
            Directory.Delete(path, true)

let private semver value =
    SemVer.tryParse value
    |> Option.defaultWith (fun () -> failtestf "Invalid test SemVer: %s" value)

let private declaration id primitive nullable position prefix =
    CommandInputParameter.create(
        id,
        CommandInputType.create(primitive, nullable),
        CommandInputBinding.create(Position = position, Prefix = prefix)
    )

let private cachedPackage name version declarations =
    let parsed = semver version

    let metadata =
        ValidationPackageMetadata.create(
            name,
            "Configured package",
            "Configured package used by child execution tests.",
            parsed.Major,
            parsed.Minor,
            parsed.Patch,
            "FSharp",
            PreReleaseVersionSuffix = parsed.PreRelease,
            BuildMetadataVersionSuffix = parsed.BuildMetadata,
            Inputs = declarations
        )

    CachedValidationPackage.create(
        $"{name}@{version}.fsx",
        DateTimeOffset.Parse("2026-08-20T00:00:00Z"),
        $"C:/cache/{name}@{version}.fsx",
        metadata
    )

let private writeUtf8 (path: string) (content: string) includeBom =
    let contentBytes = UTF8Encoding(false).GetBytes(content)

    let bytes =
        if includeBom then
            Array.concat [ [| 0xEFuy; 0xBBuy; 0xBFuy |]; contentBytes ]
        else
            contentBytes

    File.WriteAllBytes(path, bytes)

let private expectConfigurationError expected action =
    try
        action ()
        failtestf "Expected configuration failure containing '%s'." expected
    with ConfigurationException message ->
        Expect.stringContains message expected "Configuration diagnostic"

let private canonicalConfig name version policy inputs =
    $"""$schema: "{schema}"
validation_packages:
  - name: {name}
    version: {version}
    roll_forward: {policy}
{inputs}"""

[<Tests>]
let tests =
    testList "configured validation child execution" [
        test "selects the exact cached version without registry resolution" {
            withTemporaryDirectory (fun root ->
                let configPath = Path.Combine(root, "validation_packages.yml")
                writeUtf8
                    configPath
                    (canonicalConfig "configured" "1.2.3" "latest_patch" "")
                    false

                let cache =
                    PackageCache(
                        [
                            cachedPackage "configured" "1.2.3" Array.empty
                            cachedPackage "configured" "1.2.7" Array.empty
                            cachedPackage "configured" "1.3.0" Array.empty
                        ]
                    )

                let prepared =
                    ChildExecution.prepare
                        configPath
                        None
                        "configured"
                        "1.2.7"
                        cache

                Expect.equal
                    (CachedValidationPackage.getSemanticVersionString prepared.Package)
                    "1.2.7"
                    "The parent-selected exact cache entry is used"

                expectConfigurationError "outside the requested 'latest_patch' band" (fun () ->
                    ChildExecution.prepare
                        configPath
                        None
                        "configured"
                        "1.3.0"
                        cache
                    |> ignore
                )
            )
        }

        test "verifies exact bytes, lowercase digests, BOMs, and interactive omission" {
            withTemporaryDirectory (fun root ->
                let configPath = Path.Combine(root, "validation_packages.yml")
                let content = canonicalConfig "configured" "1.0.0" "disable" ""
                writeUtf8 configPath content true

                let cache =
                    PackageCache(
                        [ cachedPackage "configured" "1.0.0" Array.empty ]
                    )

                let digest = (ValidationConfigFile.load configPath).Sha256

                let withDigest =
                    ChildExecution.prepare
                        configPath
                        (Some digest)
                        "configured"
                        "1.0.0"
                        cache

                Expect.equal withDigest.ConfigSha256 digest "BOM participates in the digest"

                ChildExecution.prepare
                    configPath
                    None
                    "configured"
                    "1.0.0"
                    cache
                |> ignore

                expectConfigurationError "64 lowercase hexadecimal" (fun () ->
                    ChildExecution.prepare
                        configPath
                        (Some(digest.ToUpperInvariant()))
                        "configured"
                        "1.0.0"
                        cache
                    |> ignore
                )

                writeUtf8 configPath (content + "\n") true

                expectConfigurationError "configuration bytes changed" (fun () ->
                    ChildExecution.prepare
                        configPath
                        (Some digest)
                        "configured"
                        "1.0.0"
                        cache
                    |> ignore
                )
            )
        }

        test "materializes safe invariant argument tokens in position and id order" {
            withTemporaryDirectory (fun root ->
                let declarations =
                    [|
                        declaration "zeta" CwlPrimitive.String false 5 "--zeta"
                        declaration "flag" CwlPrimitive.Boolean false 1 "--flag"
                        declaration "enabled" CwlPrimitive.Boolean false 1 "--enabled"
                        declaration "alpha" CwlPrimitive.Int false 5 "--alpha"
                        declaration "int-max" CwlPrimitive.Int false 5 "--int-max"
                        declaration "long-max" CwlPrimitive.Long false 6 "--long-max"
                        declaration "long-min" CwlPrimitive.Long false 6 "--long-min"
                        declaration "optional" CwlPrimitive.String true 0 "--optional"
                        declaration "empty" CwlPrimitive.String false 2 "--empty"
                        declaration "leading" CwlPrimitive.String false 3 "--leading"
                        declaration "hostile" CwlPrimitive.String false 4 "--hostile"
                        declaration "unicode" CwlPrimitive.String false 4 "--unicode"
                    |]

                let configPath = Path.Combine(root, "validation_packages.yml")

                let configuredInputs =
                    """    inputs:
      zeta: "hello world"
      flag: false
      enabled: true
      alpha: -2147483648
      int-max: 2147483647
      long-max: 9223372036854775807
      long-min: -9223372036854775808
      optional: null
      empty: ""
      leading: "--literal"
      hostile: "$(touch injected); & <xml>"
      unicode: "Grüße 🌱"
"""

                writeUtf8
                    configPath
                    (canonicalConfig
                        "configured"
                        "1.0.0"
                        "disable"
                        configuredInputs)
                    false

                let cache =
                    PackageCache(
                        [ cachedPackage "configured" "1.0.0" declarations ]
                    )

                let prepared =
                    ChildExecution.prepare
                        configPath
                        None
                        "configured"
                        "1.0.0"
                        cache

                Expect.sequenceEqual
                    prepared.PackageArguments
                    [|
                        "--enabled"
                        "--empty"
                        ""
                        "--leading"
                        "--literal"
                        "--hostile"
                        "$(touch injected); & <xml>"
                        "--unicode"
                        "Grüße 🌱"
                        "--alpha"
                        "-2147483648"
                        "--int-max"
                        "2147483647"
                        "--zeta"
                        "hello world"
                        "--long-max"
                        "9223372036854775807"
                        "--long-min"
                        "-9223372036854775808"
                    |]
                    "False and null are omitted; every other value remains one token"

                let processArguments =
                    PackageProcessArguments.create
                        "/arc path"
                        "/output path"
                        None
                        None
                        prepared.PackageArguments

                Expect.sequenceEqual
                    processArguments[..3]
                    [| "-i"; "/arc path"; "-o"; "/output path" |]
                    "Configured tokens follow the standard argument tokens"
                Expect.sequenceEqual
                    processArguments[4..]
                    prepared.PackageArguments
                    "No configured token is joined or interpreted"

                let processResult =
                    FSharpScript.runWithArgs fsharpTestScriptArgsPath processArguments

                Expect.equal processResult.ExitCode 0 "The configured argv executes"
                Expect.isEmpty processResult.Errors "The configured argv creates no child error"

                let processOutput = String.concat "\n" processResult.Messages

                Expect.stringContains processOutput "\"\"" "Empty argv survives"
                Expect.stringContains processOutput "--literal" "Leading dashes survive"
                Expect.stringContains processOutput "Grüße 🌱" "Unicode survives"
                Expect.stringContains processOutput "hello world" "Whitespace survives"
                Expect.stringContains
                    processOutput
                    "$(touch injected); & <xml>"
                    "Shell-looking text remains literal"
            )
        }

        test "rechecks exact and legacy intent without requiring the current latest version" {
            withTemporaryDirectory (fun root ->
                let cache =
                    PackageCache(
                        [
                            cachedPackage "configured" "1.0.0" Array.empty
                            cachedPackage "configured" "1.0.1" Array.empty
                            cachedPackage "legacy" "2.0.0" Array.empty
                            cachedPackage "legacy" "2.1.0-rc.1" Array.empty
                        ]
                    )

                let exactPath = Path.Combine(root, "exact.yml")
                writeUtf8
                    exactPath
                    (canonicalConfig "configured" "1.0.0" "disable" "")
                    false

                expectConfigurationError "does not equal the exact requested version" (fun () ->
                    ChildExecution.prepare
                        exactPath
                        None
                        "configured"
                        "1.0.1"
                        cache
                    |> ignore
                )

                let legacyPath = Path.Combine(root, "legacy.yml")
                writeUtf8
                    legacyPath
                    """validation_packages:
  - name: legacy
"""
                    false

                ChildExecution.prepare
                    legacyPath
                    None
                    "legacy"
                    "2.0.0"
                    cache
                |> ignore

                expectConfigurationError "requires a stable resolved version" (fun () ->
                    ChildExecution.prepare
                        legacyPath
                        None
                        "legacy"
                        "2.1.0-rc.1"
                        cache
                    |> ignore
                )
            )
        }

        test "legacy selections revalidate the exact cached declarations" {
            withTemporaryDirectory (fun root ->
                let configPath = Path.Combine(root, "legacy.yml")
                writeUtf8
                    configPath
                    """validation_packages:
  - name: legacy
"""
                    false

                let requiredDeclaration =
                    declaration "required" CwlPrimitive.String false 0 "--required"

                let cache =
                    PackageCache(
                        [
                            cachedPackage
                                "legacy"
                                "2.0.0"
                                [| requiredDeclaration |]
                        ]
                    )

                expectConfigurationError "inputs['required'] is required" (fun () ->
                    ChildExecution.prepare
                        configPath
                        None
                        "legacy"
                        "2.0.0"
                        cache
                    |> ignore
                )
            )
        }

        test "rejects missing and corrupted exact cache entries as configuration errors" {
            withTemporaryDirectory (fun root ->
                let configPath = Path.Combine(root, "validation_packages.yml")
                writeUtf8
                    configPath
                    (canonicalConfig "configured" "1.0.0" "disable" "")
                    false

                expectConfigurationError "is not installed" (fun () ->
                    ChildExecution.prepare
                        configPath
                        None
                        "configured"
                        "1.0.0"
                        (PackageCache())
                    |> ignore
                )

                let corrupted = cachedPackage "configured" "1.0.0" Array.empty
                let cache = PackageCache([ corrupted ])
                corrupted.Metadata.MajorVersion <- -1

                expectConfigurationError "invalid package identity" (fun () ->
                    ChildExecution.prepare
                        configPath
                        None
                        "configured"
                        "1.0.0"
                        cache
                    |> ignore
                )
            )
        }

        test "rejects missing source selections and incompatible minor or legacy intent" {
            withTemporaryDirectory (fun root ->
                let cache =
                    PackageCache(
                        [
                            cachedPackage "configured" "1.9.9" Array.empty
                            cachedPackage "configured" "2.0.0" Array.empty
                            cachedPackage "legacy" "2.0.1" Array.empty
                        ]
                    )

                let minorPath = Path.Combine(root, "minor.yml")
                writeUtf8
                    minorPath
                    (canonicalConfig "configured" "1.2.3" "latest_minor" "")
                    false

                ChildExecution.prepare
                    minorPath
                    None
                    "configured"
                    "1.9.9"
                    cache
                |> ignore

                expectConfigurationError "outside the requested 'latest_minor' band" (fun () ->
                    ChildExecution.prepare
                        minorPath
                        None
                        "configured"
                        "2.0.0"
                        cache
                    |> ignore
                )

                expectConfigurationError "does not select package 'missing'" (fun () ->
                    ChildExecution.prepare
                        minorPath
                        None
                        "missing"
                        "1.9.9"
                        cache
                    |> ignore
                )

                let legacyPath = Path.Combine(root, "legacy-exact.yml")
                writeUtf8
                    legacyPath
                    """validation_packages:
  - name: legacy
    version: 2.0.0
"""
                    false

                expectConfigurationError "does not equal the exact legacy version" (fun () ->
                    ChildExecution.prepare
                        legacyPath
                        None
                        "legacy"
                        "2.0.1"
                        cache
                    |> ignore
                )
            )
        }
    ]
