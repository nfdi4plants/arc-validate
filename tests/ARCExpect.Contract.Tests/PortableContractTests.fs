module ARCExpect.Contract.Tests.PortableContractTests

open ARCExpect
open ARCExpect.Badge
open ARCExpect.JUnit
open Fable.Pyxpecto
open Thoth.Json.Core
open ValidationPackage.Model

let private passed name =
    CaseResult.create([| name |], CaseOutcome.passed(), DurationMilliseconds = 1.0)

let private failed name message =
    CaseResult.create([| name |], CaseOutcome.failed(message), DurationMilliseconds = 250.0)

let private errored name message stackTrace =
    CaseResult.create(
        [| name |],
        CaseOutcome.errored(message, StackTrace = stackTrace),
        DurationMilliseconds = 500.0
    )

let private skipped name message =
    CaseResult.create([| name |], CaseOutcome.skipped(message))

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

let private summaryWithSource =
    ValidationSummary.create(
        summary.Critical,
        summary.NonCritical,
        summary.ValidationPackage,
        ?Payload = summary.Payload,
        SourceBranch = "feature/source-metadata",
        SourceCommitHash = "abc123&456"
    )

let private allOutcomes =
    RunSummary.create(
        [|
            passed "passed"
            failed "failed" "assertion failed"
            errored "errored" "unexpected error" "portable stack"
            skipped "skipped" "not applicable"
        |],
        SuiteName = "all-outcomes"
    )

let private commandInput id primitive isNullable position prefix =
    CommandInputParameter.create(
        id,
        CommandInputType.create(primitive, IsNullable = isNullable),
        CommandInputBinding.create(Position = position, Prefix = prefix)
    )

let private argumentMetadata inputs =
    ValidationPackageMetadata.create(
        "argument-contract",
        "Argument contract",
        "Exercises standard and CWL package arguments.",
        1,
        0,
        0,
        "FSharp",
        Inputs = inputs
    )

let tests =
    testList "portable ARCExpect contracts" [
#if FABLE_COMPILER_PYTHON
        testCase "Python PACKAGE_METADATA runtime values retain YAML frontmatter" <| fun () ->
            let packageMetadata =
                """
---
Name: python-runtime-frontmatter
Summary: Python runtime frontmatter
Description: The bound string no longer contains its source-code triple quotes.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
---
"""

            let actual = Setup.Metadata(packageMetadata)

            Expect.equal actual.Name "python-runtime-frontmatter" "metadata name"
            Expect.equal actual.ProgrammingLanguage "Python" "source language"
#else
#if FABLE_COMPILER_JAVASCRIPT
        testCase "JavaScript metadata setup remains explicit until frontmatter is defined" <| fun () ->
            Expect.throws
                (fun () -> Setup.Metadata("not-defined") |> ignore)
                "JavaScript frontmatter must not silently use another language format"
#else
        testCase "FSharp PACKAGE_METADATA selects FSharp frontmatter" <| fun () ->
            let actual =
                Setup.Metadata(
                    """(*
---
Name: fsharp-runtime-frontmatter
Summary: FSharp runtime frontmatter
Description: The target selects the FSharp frontmatter codec.
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Publish: false
---
*)"""
                )

            Expect.equal actual.Name "fsharp-runtime-frontmatter" "metadata name"
            Expect.equal actual.ProgrammingLanguage "FSharp" "source language"
#endif
#endif

        testCase "run summaries expose framework-neutral case outcomes" <| fun () ->
            Expect.equal summary.Critical.Total 1 "Critical total"
            Expect.equal summary.Critical.Passed 1 "Critical passed"
            Expect.isFalse summary.Critical.HasFailures "Critical success"
            Expect.equal summary.NonCritical.Failed 1 "Noncritical failed"
            Expect.isTrue summary.NonCritical.HasFailures "Noncritical failure"
            Expect.equal summary.NonCritical.Cases[0].FullName "[ noncritical ]" "Portable case name"

            Expect.equal allOutcomes.Total 4 "All outcomes total"
            Expect.equal allOutcomes.Passed 1 "Passed count"
            Expect.equal allOutcomes.Failed 1 "Failed count"
            Expect.equal allOutcomes.Errored 1 "Errored count"
            Expect.equal allOutcomes.Skipped 1 "Skipped count"
            Expect.isTrue allOutcomes.HasFailures "Errors and failures make the run unsuccessful"

        testCase "summary JSON preserves the established wire shape" <| fun () ->
            let json = ValidationSummary.toJson summary
            Expect.isTrue (json.Contains("\"Critical\":{\"HasFailures\":false,\"Total\":1")) "Critical wire fields"
            Expect.isTrue (json.Contains("\"CQCHookEndpoint\":\"https://example.org/hook\"")) "Hook field"
            Expect.isTrue (json.Contains("\"Payload\":{\"runtime\":\"portable\",\"count\":2}")) "Payload field"
            Expect.isFalse (json.Contains("SourceBranch")) "Legacy summaries omit source branch"
            Expect.isFalse (json.Contains("SourceCommitHash")) "Legacy summaries omit source commit"

            let decoded = ValidationSummary.fromJson json
            Expect.equal decoded.Critical.Passed 1 "Decoded critical aggregate"
            Expect.equal decoded.NonCritical.Failed 1 "Decoded noncritical aggregate"
            Expect.equal decoded.ValidationPackage package "Decoded package"
            Expect.equal decoded.Payload summary.Payload "Decoded payload"
            Expect.equal decoded.SourceBranch None "Missing source branch remains optional"
            Expect.equal decoded.SourceCommitHash None "Missing source commit remains optional"

        testCase "summary JSON roundtrips optional source metadata" <| fun () ->
            let json = ValidationSummary.toJson summaryWithSource
            Expect.isTrue (json.Contains("\"SourceBranch\":\"feature/source-metadata\"")) "Source branch field"
            Expect.isTrue (json.Contains("\"SourceCommitHash\":\"abc123&456\"")) "Source commit field"

            let decoded = ValidationSummary.fromJson json
            Expect.equal decoded.SourceBranch summaryWithSource.SourceBranch "Decoded source branch"
            Expect.equal decoded.SourceCommitHash summaryWithSource.SourceCommitHash "Decoded source commit"

        testCase "summary counts reject inconsistent aggregate data" <| fun () ->
            Expect.throws
                (fun () ->
                    ValidationResult.fromCounts(1, 1, 1, 0)
                    |> ignore
                )
                "Outcome counts cannot exceed total"

            let inconsistentJson =
                """{"Critical":{"HasFailures":false,"Total":1,"Passed":0,"Failed":1,"Errored":0},"NonCritical":{"HasFailures":false,"Total":0,"Passed":0,"Failed":0,"Errored":0},"ValidationPackage":{"Name":"invalid","Version":"1.0.0","Summary":"invalid","Description":"invalid"}}"""

            Expect.throws
                (fun () ->
                    ValidationSummary.fromJson(inconsistentJson)
                    |> ignore
                )
                "HasFailures must agree with outcome counts"

        testCase "JUnit output maps outcomes and escapes XML" <| fun () ->
            let combined = RunSummary.combine [| summary.Critical; summary.NonCritical; allOutcomes |]
            let xml = ARCExpect.JUnit.Writer.toXml(combined, SuiteName = "suite & contract")
            Expect.isTrue (xml.Contains("name=\"suite &amp; contract\"")) "Suite name is escaped"
            Expect.isTrue (xml.Contains("<failure message=\"expected &lt;actual&gt; &amp; more\" />")) "Failure is escaped"
            Expect.isTrue (xml.Contains("<error message=\"unexpected error\" />")) "Error is encoded"
            Expect.isTrue (xml.Contains("<skipped message=\"not applicable\" />")) "Skipped case is encoded"
            Expect.isTrue (xml.Contains("time=\"0.250\"")) "Duration is invariant"
            Expect.isTrue (xml.Contains("tests=\"6\"")) "Suite test count"
            Expect.isTrue (xml.Contains("failures=\"2\"")) "Suite failure count"
            Expect.isTrue (xml.Contains("errors=\"1\"")) "Suite error count"
            Expect.isTrue (xml.Contains("skipped=\"1\"")) "Suite skipped count"
            Expect.isFalse (xml.Contains("<properties>")) "Source properties are omitted by default"
            Expect.isTrue
                (xml.IndexOf("[ critical ]") < xml.IndexOf("[ noncritical ]"))
                "Execution order is retained"

            let verboseXml =
                ARCExpect.JUnit.Writer.toXml(allOutcomes, Verbose = true)
            Expect.isTrue
                (verboseXml.Contains("portable stack"))
                "Verbose JUnit includes escaped stack trace content"

            let xmlWithSource =
                ARCExpect.JUnit.Writer.toXml(
                    combined,
                    SourceBranch = "feature/<source>",
                    SourceCommitHash = "abc123&456"
                )
            Expect.isTrue (xmlWithSource.Contains("<properties>")) "JUnit properties container"
            Expect.isTrue (xmlWithSource.Contains("name=\"SourceBranch\" value=\"feature/&lt;source&gt;\"")) "JUnit source branch"
            Expect.isTrue (xmlWithSource.Contains("name=\"SourceCommitHash\" value=\"abc123&amp;456\"")) "JUnit source commit"

        testCase "badge output is deterministic and escapes labels" <| fun () ->
            let svg = ARCExpect.Badge.Writer.toSvg(summary, "portable & result")
            Expect.isTrue (svg.Contains("id=\"arc-validate-badge\"")) "Stable mask id"
            Expect.isTrue (svg.Contains("portable &amp; result")) "Label is escaped"
            Expect.isTrue (svg.Contains("1/2")) "Passed and total counts"
            Expect.isFalse (svg.Contains("<metadata>")) "Source metadata is omitted by default"

            let svgWithSource = ARCExpect.Badge.Writer.toSvg(summaryWithSource, "portable result")
            Expect.isTrue (svgWithSource.Contains("<metadata>")) "SVG metadata container"
            Expect.isTrue (svgWithSource.Contains("SourceBranch=\"feature/source-metadata\"")) "SVG source branch"
            Expect.isTrue (svgWithSource.Contains("SourceCommitHash=\"abc123&amp;456\"")) "SVG source commit"

        testCase "badge defaults use red, orange, and green lower bounds" <| fun () ->
            let badgeSummary critical nonCritical =
                ValidationSummary.create(
                    critical,
                    nonCritical,
                    package
                )

            let nonePassed =
                badgeSummary
                    (ValidationResult.create(Array.empty))
                    (ValidationResult.create([| failed "failed" "noncritical" |]))
                |> fun value -> ARCExpect.Badge.Writer.toSvg(value, "none")

            let halfPassed =
                badgeSummary
                    (ValidationResult.create([| passed "passed" |]))
                    (ValidationResult.create([| skipped "skipped" "pending" |]))
                |> fun value -> ARCExpect.Badge.Writer.toSvg(value, "half")

            let allPassed =
                badgeSummary
                    (ValidationResult.create([| passed "passed" |]))
                    (ValidationResult.create(Array.empty))
                |> fun value -> ARCExpect.Badge.Writer.toSvg(value, "all")

            Expect.isTrue (nonePassed.Contains("fill=\"#E05D44\"")) "Zero passed is red"
            Expect.isTrue (halfPassed.Contains("fill=\"#FFA500\"")) "Half passed is orange"
            Expect.isTrue (allPassed.Contains("fill=\"#4C1\"")) "All passed is green"

        testCase "package arguments parse standard and CWL inputs without evaluating values" <| fun () ->
            let metadata =
                argumentMetadata [|
                    commandInput "test" CwlPrimitive.Boolean true 0 "--test"
                    commandInput "count" CwlPrimitive.Int false 0 "--count"
                    commandInput "ratio" CwlPrimitive.Double false 0 "--ratio"
                    commandInput "echo" CwlPrimitive.String true 0 "--echo"
                    commandInput "label" CwlPrimitive.String false 1 "--label"
                |]

            let hostileValue = "\"; $(touch injected) & <xml> `literal`"

            let actual =
                PackageArguments.parse(
                    metadata,
                    [|
                        "-i"
                        "/arc path"
                        "-o"
                        "/output path"
                        "--source-branch"
                        "feature/argument-boundary"
                        "--source-commit-hash"
                        "abc123"
                        "--test"
                        "--count"
                        "42"
                        "--ratio"
                        "1.25e2"
                        "--echo"
                        hostileValue
                        "--label"
                        "positional label"
                    |]
                )

            Expect.equal actual.ArcDirectory "/arc path" "ARC directory"
            Expect.equal actual.OutputDirectory "/output path" "Output directory"
            Expect.equal actual.SourceBranch (Some "feature/argument-boundary") "Source branch"
            Expect.equal actual.SourceCommitHash (Some "abc123") "Source commit"
            Expect.equal (actual.TryGetBoolean "test") (Some true) "Boolean flag"
            Expect.equal (actual.GetInt "count") 42 "Integer input"
            Expect.equal (actual.GetDouble "ratio") 125.0 "Double input"
            Expect.equal (actual.TryGetString "echo") (Some hostileValue) "Hostile-looking text stays literal"
            Expect.equal (actual.GetString "label") "positional label" "String input"

        testCase "package arguments apply CWL optionality and strict validation" <| fun () ->
            let metadata =
                argumentMetadata [|
                    commandInput "enabled" CwlPrimitive.Boolean false 0 "--enabled"
                    commandInput "optional" CwlPrimitive.String true 0 "--optional"
                |]

            let actual = PackageArguments.parse(metadata, [| "-i"; "/arc"; "-o"; "/out" |])
            Expect.isFalse (actual.GetBoolean "enabled") "Absent required boolean means false"
            Expect.equal (actual.TryGetString "optional") None "Absent nullable input remains optional"

            Expect.throws
                (fun () ->
                    PackageArguments.parse(metadata, [| "-i"; "/arc"; "-o"; "/out"; "--unknown" |])
                    |> ignore
                )
                "Unknown arguments must fail"

            Expect.throws
                (fun () ->
                    PackageArguments.parse(metadata, [| "-i"; "/arc"; "-i"; "/other"; "-o"; "/out" |])
                    |> ignore
                )
                "Duplicate standard arguments must fail"

            let invalidTypeMetadata =
                argumentMetadata [|
                    commandInput "count" CwlPrimitive.Int false 0 "--count"
                |]

            Expect.throws
                (fun () ->
                    PackageArguments.parse(
                        invalidTypeMetadata,
                        [| "-i"; "/arc"; "-o"; "/out"; "--count"; "not-an-int" |]
                    )
                    |> ignore
                )
                "Malformed typed values must fail"

            Expect.throws
                (fun () ->
                    let decimalMetadata =
                        argumentMetadata [|
                            commandInput "ratio" CwlPrimitive.Double false 0 "--ratio"
                        |]

                    PackageArguments.parse(
                        decimalMetadata,
                        [| "-i"; "/arc"; "-o"; "/out"; "--ratio"; "1,25" |]
                    )
                    |> ignore
                )
                "Numeric inputs must use invariant decimal syntax"

            let reservedMetadata =
                argumentMetadata [|
                    commandInput "shadow" CwlPrimitive.String true 0 "-i"
                |]

            Expect.throws
                (fun () ->
                    PackageArguments.parse(reservedMetadata, [| "-i"; "/arc"; "-o"; "/out" |])
                    |> ignore
                )
                "CWL inputs must not shadow standard arguments"

        testCaseAsync "top-level Execute runs Pyxpecto validation packages" <| async {
            let metadata =
                ValidationPackage.Model.ValidationPackageMetadata.create(
                    "portable-execute",
                    "Portable Execute",
                    "Runs the shared Pyxpecto adapter.",
                    1,
                    2,
                    3,
                    "FSharp"
                )
            let validationPackage =
                Setup.ValidationPackage(
                    metadata = metadata,
                    CriticalValidationCases = [|
                        testCase "sync pass" <| fun () -> ()
                        testCaseAsync "async pass" <| async { return () }
                    |],
                    NonCriticalValidationCases = [|
                        testCase "captured failure" <| fun () ->
                            Expect.equal 1 2 "nested assertion"
                        testCase "captured error" <| fun () ->
                            failwith "nested error"
                        ptestCase "pending case" <| fun () -> ()
                    |]
                )

            let! actual =
                Execute.Validation(
                    validationPackage,
                    Payload = Json.Object [ "runtime", Json.String "portable-execute" ],
                    SourceBranch = "dev",
                    SourceCommitHash = "0123456789"
                )

            Expect.equal actual.Critical.Total 2 "Critical total"
            Expect.equal actual.Critical.Passed 2 "Critical passed"
            Expect.equal actual.NonCritical.Total 3 "Noncritical total"
            Expect.equal actual.NonCritical.Failed 1 "Captured assertion failure"
            Expect.equal actual.NonCritical.Errored 1 "Captured unexpected error"
            Expect.equal actual.NonCritical.Skipped 1 "Captured pending case"
            Expect.equal actual.ValidationPackage.Name metadata.Name "Package metadata"
            Expect.equal
                actual.Payload
                (Some(Json.Object [ "runtime", Json.String "portable-execute" ]))
                "Portable payload"
            Expect.equal actual.SourceBranch (Some "dev") "Portable source branch"
            Expect.equal actual.SourceCommitHash (Some "0123456789") "Portable source commit"

            let focusedPackage =
                Setup.ValidationPackage(
                    metadata,
                    CriticalValidationCases = [|
                        testCase "unfocused" <| fun () -> ()
                        ftestCase "focused" <| fun () -> ()
                    |]
                )
            let! focused = Execute.Validation(focusedPackage)
            Expect.equal focused.Critical.Passed 1 "Focused case ran"
            Expect.equal focused.Critical.Skipped 1 "Unfocused case was represented"
        }
    ]
