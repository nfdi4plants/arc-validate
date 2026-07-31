module ReferenceObjects

open ControlledVocabulary
open ARCExpect
open ValidationPackage.Model
open Thoth.Json.Core

open Expecto

module TestCase =

    let dummyTestWillPass = testCase "dummyTest1" (fun _ -> Expect.isTrue true "is not true")
    let dummyTestWillFail = testCase "dummyTest2" (fun _ -> Expect.isTrue false "is not true") 

module CvTerms =

    let ``Investigation Person First Name`` = CvTerm.create("INVMSO:00000023","Investigation Person First Name","INVMSO")

    let ``Investigation Person Email`` = CvTerm.create("INVMSO:00000025","Investigation Person Email","INVMSO")
    
    let ``Investigation Description`` = CvTerm.create("INVMSO:00000010","Investigation Description","INVMSO")

module CvParams =
    
    let ``Empty Value`` = CvParam(CvTerms.``Investigation Person First Name``, ParamValue.Value "")

    let ``Investigation Person First Name`` = CvParam(CvTerms.``Investigation Person First Name``, ParamValue.Value "Kevin")

    let ``Investigation Person Email (valid)`` = CvParam(CvTerms.``Investigation Person Email``, ParamValue.Value "yes@yes.com")

    let ``Investigation Person Email (invalid)`` = CvParam(CvTerms.``Investigation Person Email``, ParamValue.Value "nope")

    let ``Investigation Description`` = CvParam(CvTerms.``Investigation Description`` , ParamValue.Value "Hello this is a description")

    let ``Investigation Description Section Key`` = CvParam(CvTerms.``Investigation Description`` , ParamValue.CvValue ARCTokenization.Terms.StructuralTerms.metadataSectionKey)

module ValidationResult =
    
    let allPassed = ValidationResult.fromCounts(1, 1, 0, 0)

    let allFailed = ValidationResult.fromCounts(1, 0, 1, 0)

module ValidationPackageSummary = 

    let noHook =
        ValidationPackageSummary.create(
            "test",
            "1.0.0",
            "A package without CQC hook.",
            "A package without CQC hook. More text here."
        )

    let withHook =
        ValidationPackageSummary.create(
            "test",
            "1.0.0",
            "A package with CQC hook.",
            "A package with CQC hook. More text here.",
            CQCHookEndpoint = "http://test.com"
        )

module ValidationSummary =

    let private create critical nonCritical validationPackage payload =
        ValidationSummary.create(
            critical,
            nonCritical,
            validationPackage,
            ?Payload = payload
        )

    let allPassedNoHook = 
        create ValidationResult.allPassed ValidationResult.allPassed ValidationPackageSummary.noHook None

    let allPassedNoHookJson = """{"Critical":{"HasFailures":false,"Total":1,"Passed":1,"Failed":0,"Errored":0},"NonCritical":{"HasFailures":false,"Total":1,"Passed":1,"Failed":0,"Errored":0},"ValidationPackage":{"Name":"test","Version":"1.0.0","Summary":"A package without CQC hook.","Description":"A package without CQC hook. More text here."}}"""

    let allPassedWithHook = 
        create ValidationResult.allPassed ValidationResult.allPassed ValidationPackageSummary.withHook None

    let allPassedWithHookJson = """{"Critical":{"HasFailures":false,"Total":1,"Passed":1,"Failed":0,"Errored":0},"NonCritical":{"HasFailures":false,"Total":1,"Passed":1,"Failed":0,"Errored":0},"ValidationPackage":{"Name":"test","Version":"1.0.0","Summary":"A package with CQC hook.","Description":"A package with CQC hook. More text here.","CQCHookEndpoint":"http://test.com"}}""" 

    let allPassedWithPayload = 
        create
            ValidationResult.allPassed
            ValidationResult.allPassed
            ValidationPackageSummary.withHook
            (Some (Json.Object [ "key1", Json.String "value1"; "key2", Json.Number 2.0 ]))

    let allPassedWithPayloadJson = """{"Critical":{"HasFailures":false,"Total":1,"Passed":1,"Failed":0,"Errored":0},"NonCritical":{"HasFailures":false,"Total":1,"Passed":1,"Failed":0,"Errored":0},"ValidationPackage":{"Name":"test","Version":"1.0.0","Summary":"A package with CQC hook.","Description":"A package with CQC hook. More text here.","CQCHookEndpoint":"http://test.com"},"Payload":{"key1":"value1","key2":2}}""" 

    let allFailedNoHook = 
        create ValidationResult.allFailed ValidationResult.allFailed ValidationPackageSummary.noHook None

    let allFailedWithHook = 
        create ValidationResult.allFailed ValidationResult.allFailed ValidationPackageSummary.withHook None

    let nonCriticalFailedNoHook = 
        create ValidationResult.allPassed ValidationResult.allFailed ValidationPackageSummary.noHook None

    let nonCriticalFailedWithHook = 
        create ValidationResult.allPassed ValidationResult.allFailed ValidationPackageSummary.withHook None

    let criticalFailedNoHook = 
        create ValidationResult.allFailed ValidationResult.allPassed ValidationPackageSummary.noHook None

    let criticalFailedWithHook = 
        create ValidationResult.allFailed ValidationResult.allPassed ValidationPackageSummary.withHook None
         
module Frontmatter =
    
    let validNoHook = """(*
---
Name: test
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: A package without CQC hook.
Description: A package without CQC hook. More text here.
---
*)"""                                                                         .ReplaceLineEndings("\n")
    
    let validWithHook = """(*
---
Name: test
MajorVersion: 1
MinorVersion: 0
PatchVersion: 0
Summary: A package with CQC hook.
Description: A package with CQC hook. More text here.
---
*)"""                                                                         .ReplaceLineEndings("\n")

    let invalid = """
Name: invalid
MinorVersion: 0
PatchVersion: 0
Summary: My package does the thing.
Description: |
  My package does the thing.
  It does it very good, it does it very well.
  It does it very fast, it does it very swell.

"""                                                                         .ReplaceLineEndings("\n")

module ValidationPackageMetadata =
    
    let validNoHook = 
    
        ValidationPackageMetadata(
            Name = "test",
            MajorVersion = 1,
            MinorVersion = 0,
            PatchVersion = 0,
            Summary = "A package without CQC hook.",
            Description = "A package without CQC hook. More text here.",
            ProgrammingLanguage = "FSharp"
        )

    let validWithHook = 
    
        ValidationPackageMetadata(
            Name = "test",
            MajorVersion = 1,
            MinorVersion = 0,
            PatchVersion = 0,
            Summary = "A package with CQC hook.",
            Description = "A package with CQC hook. More text here.",
            ProgrammingLanguage = "FSharp",
            CQCHookEndpoint = "http://test.com"
        )

module JUnitReport =
    
    let allPassedWithHookXml = """<?xml version="1.0" encoding="utf-8"?><testsuites><testsuite name="testhost"><testcase name="[ Critical; dummyTest1 ]" time="0.000" /><testcase name="[ NonCritical; dummyTest1 ]" time="0.000" /></testsuite></testsuites>""".ReplaceLineEndings("\n")

module Badge =
    
    let allPassedWithHookBadgeSVG = """<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="150" height="20">
    <linearGradient id="b" x2="0" y2="100%">
        <stop offset="0" stop-color="#bbb" stop-opacity=".1"/>
        <stop offset="1" stop-opacity=".1"/>
    </linearGradient>
    <mask id="arc-validate-badge">
        <rect width="150" height="20" rx="3" fill="#fff"/>
    </mask>
    <g mask="url(#arc-validate-badge)">
        <path fill="#555" d="M0 0h121v20H0z"/>
        <path fill="#4C1" d="M121 0h29v20H121z"/>
        <path fill="url(#b)" d="M0 0h150v20H0z"/>
    </g>
    <g fill="#fff" text-anchor="middle" font-family="DejaVu Sans,Verdana,Geneva,sans-serif" font-size="11">
        <text x="61.5" y="15" fill="#010101" fill-opacity=".3">allPassedWithHook</text>
        <text x="60.5" y="14">allPassedWithHook</text>
    </g>
    <g fill="#fff" text-anchor="middle" font-family="DejaVu Sans,Verdana,Geneva,sans-serif" font-size="11">
        <text x="136.5" y="15" fill="#010101" fill-opacity=".3">2/2</text>
        <text x="135.5" y="14">2/2</text>
    </g>
</svg>
"""                                     .ReplaceLineEndings("\n")

    let allPassedWithHookBadgeSVGFromPipeline = """<?xml version="1.0" encoding="UTF-8"?>
<svg xmlns="http://www.w3.org/2000/svg" width="104" height="20">
    <linearGradient id="b" x2="0" y2="100%">
        <stop offset="0" stop-color="#bbb" stop-opacity=".1"/>
        <stop offset="1" stop-opacity=".1"/>
    </linearGradient>
    <mask id="arc-validate-badge">
        <rect width="104" height="20" rx="3" fill="#fff"/>
    </mask>
    <g mask="url(#arc-validate-badge)">
        <path fill="#555" d="M0 0h75v20H0z"/>
        <path fill="#4C1" d="M75 0h29v20H75z"/>
        <path fill="url(#b)" d="M0 0h104v20H0z"/>
    </g>
    <g fill="#fff" text-anchor="middle" font-family="DejaVu Sans,Verdana,Geneva,sans-serif" font-size="11">
        <text x="38.5" y="15" fill="#010101" fill-opacity=".3">test@1.0.0</text>
        <text x="37.5" y="14">test@1.0.0</text>
    </g>
    <g fill="#fff" text-anchor="middle" font-family="DejaVu Sans,Verdana,Geneva,sans-serif" font-size="11">
        <text x="90.5" y="15" fill="#010101" fill-opacity=".3">2/2</text>
        <text x="89.5" y="14">2/2</text>
    </g>
</svg>
"""                                                 .ReplaceLineEndings("\n")
