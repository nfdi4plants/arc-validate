namespace ArcValidation.TestGeneration.NonCritical.Arc

open ArcValidation
open ArcValidation.Configs

module ISA =

    open OntologyHelperFunctions
    open CheckIsaStructure
    open Expecto
    open ArcGraphModel
    open ArcGraphModel.IO
    open ErrorMessage.FailStrings
    open FSharpAux
    //open System.IO
    open CvTokenHelperFunctions

    let generateISATests (arcConfig: ArcConfig) =

        //let pathConfig = arcConfig.PathConfig

        testList "ISA" [
            testList "Semantic" [
                testList "Investigation" [
                    testList "Person" (
                        arcConfig.InvestigationContactsContainer
                        |> List.ofSeq
                        |> List.mapi (
                            fun i p ->
                                // try get person name if possible to display more precise test case name
                                let optName = 
                                    let optFN = CvContainer.tryGetPropertyStringValue "first name" p    |> Option.defaultValue "?"
                                    let optLN = CvContainer.tryGetPropertyStringValue "last name" p     |> Option.defaultValue "?"
                                    let optN = $"{optFN} {optLN}"
                                    if optN = "? ?" then "(n/a)" else optN
                                testList $"Person{i + 1} [{optName}]" [
                                    testCase "ORCID" <| fun () -> Validate.NonCritical.CvBase.Person.orcid p |> throwError FilesystemEntry.isPresent
                                ]
                        )
                    )
                ]
            ]
        ]