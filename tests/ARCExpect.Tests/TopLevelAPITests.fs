module TopLevelAPITests

open ARCExpect
open AVPRIndex
open Expecto
open TestUtils
open System.IO

[<Tests>]
let ``Toplevel API Setup tests`` =
    testList "Toplevel API tests" [
        testList "Setup_Metadata" [
            test "correct metadata is extracted from valid frontmatter string" {
                let actual = Setup.Metadata(ReferenceObjects.Frontmatter.validNoHook)
                Expect.equal actual ReferenceObjects.ValidationPackageMetadata.validNoHook "metadata was not equal"
            }
            test "incorrect frontmatter string throws" {
                Expect.throws (fun () -> Setup.Metadata(ReferenceObjects.Frontmatter.invalid) |> ignore) "did not throw"
            }
        ]
        testList "Setup_ValidationPackage" [
            test "validation package created from metadata and equivalent single values are equal" {
                let fromMetadata = 
                    Setup.ValidationPackage(
                        metadata = ReferenceObjects.ValidationPackageMetadata.validNoHook,
                        CriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                        NonCriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillFail]
                    )
                let fromValues = 
                    Setup.ValidationPackage(
                        name = "test",
                        majorVersion = 1,
                        minorVersion = 0,
                        patchVersion = 0,
                        summary = "A package without CQC hook.",
                        description = "A package without CQC hook. More text here.",
                        CriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                        NonCriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillFail]
                    )
                Expect.equal fromMetadata.Metadata.Name fromValues.Metadata.Name "metadata names were not equal"
                Expect.equal fromMetadata.Metadata.MajorVersion fromValues.Metadata.MajorVersion "metadata major versions were not equal"
                Expect.equal fromMetadata.Metadata.MinorVersion fromValues.Metadata.MinorVersion "metadata minor versions were not equal"
                Expect.equal fromMetadata.Metadata.PatchVersion fromValues.Metadata.PatchVersion "metadata patch versions were not equal"
                Expect.equal fromMetadata.Metadata.Summary fromValues.Metadata.Summary "metadata summaries were not equal"
                Expect.equal fromMetadata.Metadata.Description fromValues.Metadata.Description "metadata descriptions were not equal"
                Expect.equal fromMetadata.Metadata.Authors fromValues.Metadata.Authors "metadata authors were not equal"
                Expect.equal fromMetadata.Metadata.Publish fromValues.Metadata.Publish "metadata publish were not equal"
                Expect.equal fromMetadata.Metadata.Tags fromValues.Metadata.Tags "metadata tags were not equal"
                Expect.equal fromMetadata.Metadata.ReleaseNotes fromValues.Metadata.ReleaseNotes "metadata release notes were not equal"

                Expect.equal fromMetadata.CQCHookEndpoint fromValues.CQCHookEndpoint "hook endpoint was not equal"
            }
        ]
        testList "Execute_Validation" [
            test "resulting summary is correct noHook" {
                let actual = 
                    Setup.ValidationPackage(
                        metadata = ReferenceObjects.ValidationPackageMetadata.validNoHook,
                        CriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                        NonCriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillFail]
                    )
                    |> Execute.Validation
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.nonCriticalFailedNoHook
            }

            test "resulting summary is correct with hook" {
                let actual = 
                    Setup.ValidationPackage(
                        metadata = ReferenceObjects.ValidationPackageMetadata.validWithHook,
                        CriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                        NonCriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillFail],
                        CQCHookEndpoint = "http://test.com"
                    )
                    |> Execute.Validation
                Expect.validationSummaryEqualIgnoringOriginal actual ReferenceObjects.ValidationSummary.nonCriticalFailedWithHook
            }
        ]
        testList "Execute_SummaryCreation" [
            test "Correct summary file is created" {
                let path = Path.GetTempFileName() |> fun p -> Path.ChangeExtension(p, "json")
                Setup.ValidationPackage(
                    metadata = ReferenceObjects.ValidationPackageMetadata.validWithHook,
                    CriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                    NonCriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                    CQCHookEndpoint = "http://test.com"
                )
                |> Execute.Validation
                |> Execute.SummaryCreation path

                let actual = 
                    File.ReadAllText path

                Expect.equal actual ReferenceObjects.ValidationSummary.allPassedWithHookJson "summary files were not equal"
            }
        ]
        testList "Execute_JUnitReportCreation" [
            test "Correct report file is created" {
                let path = Path.GetTempFileName() |> fun p -> Path.ChangeExtension(p, "xml")
                Setup.ValidationPackage(
                    metadata = ReferenceObjects.ValidationPackageMetadata.validWithHook,
                    CriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                    NonCriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                    CQCHookEndpoint = "http://test.com"
                )
                |> Execute.Validation
                |> Execute.JUnitReportCreation path

                let actual = (File.ReadAllText path).ReplaceLineEndings("\n")

                Expect.equal actual ReferenceObjects.JUnitReport.allPassedWithHookXml "report files were not equal"

            }
        ]
        testList "Execute_BadgeCreation" [
            test "Correct badge file is created" {
                let path = Path.GetTempFileName() |> fun p -> Path.ChangeExtension(p, "svg")
                Setup.ValidationPackage(
                    metadata = ReferenceObjects.ValidationPackageMetadata.validWithHook,
                    CriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                    NonCriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                    CQCHookEndpoint = "http://test.com"
                )
                |> Execute.Validation
                |> Execute.BadgeCreation(path, labelText="allPassedWithHook")

                let actual = (File.ReadAllText path).ReplaceLineEndings("\n")

                Expect.equal actual ReferenceObjects.Badge.allPassedWithHookBadgeSVG "badge files were not equal"
            }
        ]
        testList "Execute_ValidationPipeline" [
            test "Correct output folder is created" {
                let path = Path.GetTempPath()
                Setup.ValidationPackage(
                    metadata = ReferenceObjects.ValidationPackageMetadata.validWithHook,
                    CriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                    NonCriticalValidationCases = [ReferenceObjects.TestCase.dummyTestWillPass],
                    CQCHookEndpoint = "http://test.com"
                )
                |> Execute.ValidationPipeline(
                    basePath = path
                )
                let expectedFolderName = Path.Combine(path, ".arc-validate-results/test@1.0.0")
                let actualSummary = File.ReadAllText (Path.Combine(expectedFolderName, "validation_summary.json"))
                let actualReport = (File.ReadAllText (Path.Combine(expectedFolderName, "validation_report.xml"))).ReplaceLineEndings("\n")
                let actualBadge = (File.ReadAllText (Path.Combine(expectedFolderName, "badge.svg"))).ReplaceLineEndings("\n")

                Expect.isTrue (Path.Exists(expectedFolderName)) "Output path did not exist (.arc-validate-results/<package>@<version>)"
                Expect.equal actualSummary ReferenceObjects.ValidationSummary.allPassedWithHookJson "summary files were not equal"
                Expect.equal actualReport ReferenceObjects.JUnitReport.allPassedWithHookXml "report files were not equal"
                Expect.equal actualBadge ReferenceObjects.Badge.allPassedWithHookBadgeSVGFromPipeline "badge files were not equal"
            }
        ]
    ]