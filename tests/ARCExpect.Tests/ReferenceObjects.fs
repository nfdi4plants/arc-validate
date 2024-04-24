module ReferenceObjects

open ControlledVocabulary
open ARCExpect

module CvTerms =

    let ``Investigation Person First Name`` = CvTerm.create("INVMSO:00000023","Investigation Person First Name","INVMSO")

    let ``Investigation Person Email`` = CvTerm.create("INVMSO:00000025","Investigation Person Email","INVMSO")        

module CvParams =
    
    let ``Empty Value`` = CvParam(CvTerms.``Investigation Person First Name``, ParamValue.Value "")

    let ``Investigation Person First Name`` = CvParam(CvTerms.``Investigation Person First Name``, ParamValue.Value "Kevin")

    let ``Investigation Person Email (valid)`` = CvParam(CvTerms.``Investigation Person Email``, ParamValue.Value "yes@yes.com")

    let ``Investigation Person Email (invalid)`` = CvParam(CvTerms.``Investigation Person Email``, ParamValue.Value "nope")

module ValidationResult =
    
    let allPassed = {
        HasFailures = false
        Total = 1
        Passed = 1
        Failed = 0
        Errored = 0
    }

    let allFailed = {
        HasFailures = true
        Total = 1
        Passed = 0
        Failed = 1
        Errored = 0
    }

module ValidationPackageSummary = 

    let noHook = {
        Name = "test"
        Version = "1.0.0"
        HookEndpoint = None
    }

    let withHook = {
        Name = "test"
        Version = "1.0.0"
        HookEndpoint = Some "http://test.com"
    }

module ValidationSummary =

    let allPassedNoHook = 
        {
            Critical = ValidationResult.allPassed
            NonCritical = ValidationResult.allPassed
            ValidationPackage = ValidationPackageSummary.noHook
        }

    let allPassedNoHookJson = """{"Critical":{"HasFailures":false,"Total":1,"Passed":1,"Failed":0,"Errored":0},"NonCritical":{"HasFailures":false,"Total":1,"Passed":1,"Failed":0,"Errored":0},"ValidationPackage":{"Name":"test","Version":"1.0.0"}}"""

    let allPassedWithHook = 
        {
            Critical = ValidationResult.allPassed
            NonCritical = ValidationResult.allPassed
            ValidationPackage = ValidationPackageSummary.withHook
        }

    let allPassedWithHookJson = """{"Critical":{"HasFailures":false,"Total":1,"Passed":1,"Failed":0,"Errored":0},"NonCritical":{"HasFailures":false,"Total":1,"Passed":1,"Failed":0,"Errored":0},"ValidationPackage":{"Name":"test","Version":"1.0.0","HookEndpoint":"http://test.com"}}""" 

    let allFailedNoHook = 
        {
            Critical = ValidationResult.allFailed
            NonCritical = ValidationResult.allFailed
            ValidationPackage = ValidationPackageSummary.noHook
        }

    let allFailedWithHook = 
        {
            Critical = ValidationResult.allFailed
            NonCritical = ValidationResult.allFailed
            ValidationPackage = ValidationPackageSummary.withHook
        }

    let nonCriticalFailedNoHook = 
        {
            Critical = ValidationResult.allPassed
            NonCritical = ValidationResult.allFailed
            ValidationPackage = ValidationPackageSummary.noHook
        }

    let nonCriticalFailedWithHook = 
        {
            Critical = ValidationResult.allPassed
            NonCritical = ValidationResult.allFailed
            ValidationPackage = ValidationPackageSummary.withHook
        }

    let criticalFailedNoHook = 
        {
            Critical = ValidationResult.allFailed
            NonCritical = ValidationResult.allPassed
            ValidationPackage = ValidationPackageSummary.noHook
        }

    let criticalFailedWithHook = 
        {
            Critical = ValidationResult.allFailed
            NonCritical = ValidationResult.allPassed
            ValidationPackage = ValidationPackageSummary.withHook
        }
