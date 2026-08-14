module ExecutionPlanTests

open System
open System.IO
open System.Text.Json
open Expecto
open Json.Schema
open ARCValidate
open ARCValidate.Configuration

let private repositoryRoot =
    Path.GetFullPath(Path.Combine(__SOURCE_DIRECTORY__, "..", "..", ".."))

let private schemaPath =
    Path.Combine(repositoryRoot, "schemas", "validation_plan.schema.json")

let private fixturePath = Path.Combine(__SOURCE_DIRECTORY__, "Fixtures", "validation_plan.json")

let private planSchema = lazy JsonSchema.FromText(File.ReadAllText(schemaPath))

let private evaluate (schema: JsonSchema) json =
    use document = JsonDocument.Parse(json: string)
    schema.Evaluate(document.RootElement)

let private expectInvalid index schema json =
    let result = evaluate schema json
    Expect.isFalse result.IsValid $"Invalid schema case {index} unexpectedly passed."

[<Tests>]
let ``execution plan contract tests`` =
    testList "execution plan contract" [
        test "encoder output is byte-for-byte stable and strict decoder roundtrips it" {
            let plan =
                {
                    ConfigSha256 = String.replicate 64 "a"
                    ArcSpecification = Some "3.0.0-draft.2"
                    ValidationPackages = [|
                        {
                            Name = "configurable-validation"
                            RequestedVersion = Some "1.2.3"
                            RollForward = ExecutionPlanRollForward.LatestPatch
                            ResolvedVersion = "1.2.7"
                        }
                    |]
                }

            let actual = ExecutionPlanCodec.encode plan
            let expected = File.ReadAllBytes(fixturePath)
            Expect.sequenceEqual actual expected "Canonical validation_plan.json bytes"
            Expect.equal (ExecutionPlanCodec.decode(ReadOnlyMemory<byte>(actual))) plan "Strict roundtrip"
        }

        test "empty and legacy plans encode the optional and null fields exactly" {
            let empty =
                {
                    ConfigSha256 = String.replicate 64 "0"
                    ArcSpecification = None
                    ValidationPackages = Array.empty
                }
                |> ExecutionPlanCodec.encode
                |> ExecutionPlanCodec.toUtf8String

            Expect.isFalse (empty.Contains("arc_specification")) "Absent ARC specification"
            Expect.isTrue (empty.Contains("\"validation_packages\": []")) "Empty package array"

            let legacy =
                {
                    ConfigSha256 = String.replicate 64 "1"
                    ArcSpecification = None
                    ValidationPackages = [|
                        {
                            Name = "legacy"
                            RequestedVersion = None
                            RollForward = ExecutionPlanRollForward.LegacyLatestStable
                            ResolvedVersion = "2.0.0"
                        }
                    |]
                }
                |> ExecutionPlanCodec.encode
                |> ExecutionPlanCodec.toUtf8String

            Expect.isTrue (legacy.Contains("\"requested_version\": null")) "Legacy null version"
            Expect.isTrue (legacy.Contains("\"roll_forward\": \"legacy_latest_stable\"")) "Legacy policy"
        }

        test "strict decoder rejects unknown schemas, duplicates, unknown fields, and invalid relationships" {
            [
                """{"$schema":"https://example.org/unknown","config_sha256":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","validation_packages":[]}"""
                $"""{{"$schema":"{ExecutionPlanCodec.SchemaUri}","config_sha256":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","config_sha256":"bbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbb","validation_packages":[]}}"""
                $"""{{"$schema":"{ExecutionPlanCodec.SchemaUri}","config_sha256":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","validation_packages":[],"extra":true}}"""
                $"""{{"$schema":"{ExecutionPlanCodec.SchemaUri}","config_sha256":"aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa","validation_packages":[{{"name":"p","requested_version":"1.2.3","roll_forward":"disable","resolved_version":"1.2.4"}}]}}"""
            ]
            |> List.iter (fun json ->
                Expect.throwsT<ConfigurationException>
                    (fun () ->
                        System.Text.Encoding.UTF8.GetBytes(json)
                        |> ReadOnlyMemory<byte>
                        |> ExecutionPlanCodec.decode
                        |> ignore
                    )
                    "Invalid plan should fail"
            )
        }

        test "digest verification rejects config bytes that differ from the plan" {
            let plan =
                {
                    ConfigSha256 = String.replicate 64 "a"
                    ArcSpecification = None
                    ValidationPackages = Array.empty
                }

            Expect.throwsT<ConfigurationException>
                (fun () ->
                    ExecutionPlanCodec.verifyConfigDigest (System.Text.Encoding.UTF8.GetBytes("different")) plan
                    |> ignore
                )
                "Digest mismatch"
        }

        test "standalone schema is valid Draft 2020-12 and validates the canonical fixture" {
            let schemaText = File.ReadAllText(schemaPath)
            let metaResult = evaluate MetaSchemas.Draft202012 schemaText
            Expect.isTrue metaResult.IsValid $"Schema meta-validation: {metaResult}"

            let schema = planSchema.Value
            let fixtureResult = evaluate schema (File.ReadAllText(fixturePath))
            Expect.isTrue fixtureResult.IsValid $"Canonical fixture: {fixtureResult}"
        }

        test "schema rejects identifier, digest, SemVer, enum, conditional, required, and closed-object failures" {
            let schema = planSchema.Value
            let hash = String.replicate 64 "a"
            let uri = ExecutionPlanCodec.SchemaUri

            [
                $"""{{"$schema":"wrong","config_sha256":"{hash}","validation_packages":[]}}"""
                $"""{{"$schema":"{uri}","config_sha256":"ABC","validation_packages":[]}}"""
                $"""{{"$schema":"{uri}","config_sha256":"{hash}","arc_specification":"3.0","validation_packages":[]}}"""
                $"""{{"$schema":"{uri}","config_sha256":"{hash}","validation_packages":[{{"name":"p","requested_version":"1.0.0","roll_forward":"major","resolved_version":"1.0.0"}}]}}"""
                $"""{{"$schema":"{uri}","config_sha256":"{hash}","validation_packages":[{{"name":"p","requested_version":null,"roll_forward":"disable","resolved_version":"1.0.0"}}]}}"""
                $"""{{"$schema":"{uri}","config_sha256":"{hash}"}}"""
                $"""{{"$schema":"{uri}","config_sha256":"{hash}","validation_packages":[],"extra":true}}"""
            ]
            |> List.iteri (fun index json -> expectInvalid index schema json)
        }

        test "schema bytes are copied unchanged beside the CLI assembly" {
            let cliDirectory = Path.GetDirectoryName(typeof<ExitCode>.Assembly.Location)
            let distributed = Path.Combine(cliDirectory, "schemas", "v1", "validation_plan.schema.json")
            Expect.isTrue (File.Exists(distributed)) "Distributed schema exists"
            Expect.sequenceEqual (File.ReadAllBytes(distributed)) (File.ReadAllBytes(schemaPath)) "Schema bytes"
        }
    ]
