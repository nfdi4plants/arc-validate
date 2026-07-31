module ARCExpect.Core.Portable.Tests.PortableContractTests

open ARCExpect
open ARCExpect.Badge
open ARCExpect.JUnit
open Fable.Pyxpecto
open Thoth.Json.Core

let private passed name =
    CaseResult.create([| name |], CaseOutcome.passed(), DurationMilliseconds = 1.0)

let private failed name message =
    CaseResult.create([| name |], CaseOutcome.failed(message), DurationMilliseconds = 250.0)

let private package =
    ValidationPackageSummary.create(
        "portable-package",
        "1.2.3-preview.1+build.4",
        "Portable summary",
        "Portable description",
        CQCHookEndpoint = "https://example.org/hook"
    )

let private summary =
    ValidationSummary.create(
        ValidationResult.create([| passed "critical" |], SuiteName = "portable-suite"),
        ValidationResult.create([| failed "noncritical" "expected <actual> & more" |], SuiteName = "portable-suite"),
        package,
        Payload = Json.Object [ "runtime", Json.String "portable"; "count", Json.Number 2.0 ]
    )

let tests =
    testList "portable ARCExpect contracts" [
        testCase "run summaries expose framework-neutral case outcomes" <| fun () ->
            Expect.equal summary.Critical.Total 1 "Critical total"
            Expect.equal summary.Critical.Passed 1 "Critical passed"
            Expect.isFalse summary.Critical.HasFailures "Critical success"
            Expect.equal summary.NonCritical.Failed 1 "Noncritical failed"
            Expect.isTrue summary.NonCritical.HasFailures "Noncritical failure"
            Expect.equal summary.NonCritical.Cases[0].FullName "[ noncritical ]" "Portable case name"

        testCase "summary JSON preserves the established wire shape" <| fun () ->
            let json = ValidationSummary.toJson summary
            Expect.isTrue (json.Contains("\"Critical\":{\"HasFailures\":false,\"Total\":1")) "Critical wire fields"
            Expect.isTrue (json.Contains("\"CQCHookEndpoint\":\"https://example.org/hook\"")) "Hook field"
            Expect.isTrue (json.Contains("\"Payload\":{\"runtime\":\"portable\",\"count\":2}")) "Payload field"

            let decoded = ValidationSummary.fromJson json
            Expect.equal decoded.Critical.Passed 1 "Decoded critical aggregate"
            Expect.equal decoded.NonCritical.Failed 1 "Decoded noncritical aggregate"
            Expect.equal decoded.ValidationPackage package "Decoded package"
            Expect.equal decoded.Payload summary.Payload "Decoded payload"

        testCase "JUnit output maps outcomes and escapes XML" <| fun () ->
            let combined = RunSummary.combine [| summary.Critical; summary.NonCritical |]
            let xml = ARCExpect.JUnit.Writer.toXml(combined, SuiteName = "suite & contract")
            Expect.isTrue (xml.Contains("name=\"suite &amp; contract\"")) "Suite name is escaped"
            Expect.isTrue (xml.Contains("<failure message=\"expected &lt;actual&gt; &amp; more\" />")) "Failure is escaped"
            Expect.isTrue (xml.Contains("time=\"0.250\"")) "Duration is invariant"

        testCase "badge output is deterministic and escapes labels" <| fun () ->
            let svg = ARCExpect.Badge.Writer.toSvg(summary, "portable & result")
            Expect.isTrue (svg.Contains("id=\"arc-validate-badge\"")) "Stable mask id"
            Expect.isTrue (svg.Contains("portable &amp; result")) "Label is escaped"
            Expect.isTrue (svg.Contains("1/2")) "Passed and total counts"
    ]
