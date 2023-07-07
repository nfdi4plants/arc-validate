namespace ArcValidation.TestGeneration.Critical.Arc

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
    open System.IO
    open CvTokenHelperFunctions

    let generateISATests (arcConfig: ArcConfig) =

        let pathConfig = arcConfig.PathConfig

        testList "ISA" [
            testList "Semantic" [
                testList "Investigation" [
                    // Validate the existence of any Person in Investigation Contacts section:
                    testCase "Contacts" <| fun () -> Validate.CvBase.contacts pathConfig.InvestigationPath arcConfig.InvestigationContactsContainer |> throwError FilesystemEntry.isPresent
                    testList "Person" (
                        arcConfig.InvestigationContactsContainer
                        |> List.ofSeq
                        |> List.mapi (
                            fun i p ->
                                // try get person name if possible to display more precise test case name
                                let optName = 
                                    let optFN = CvContainer.tryGetPropertyStringValue "given name" p    |> Option.defaultValue "?"
                                    let optLN = CvContainer.tryGetPropertyStringValue "family name" p     |> Option.defaultValue "?"
                                    let optN = $"{optFN} {optLN}"
                                    if optN = "? ?" then "(n/a)" else optN
                                // Validate the sufficiency of a Person in Investigation Contacts section (a Person is sufficient when first and last name, and email address are present):
                                testList $"Person{i + 1} [{optName}]" [
                                    testCase "First name"       <| fun () -> Validate.CvBase.Person.firstName       p |> throwError FilesystemEntry.isPresent
                                    testCase "Last name"        <| fun () -> Validate.CvBase.Person.lastName        p |> throwError FilesystemEntry.isPresent
                                    testCase "Email address"    <| fun () -> Validate.CvBase.Person.emailAddress    p |> throwError FilesystemEntry.isPresent
                                    testCase "Affiliation"      <| fun () -> Validate.CvBase.Person.affiliation     p |> throwError FilesystemEntry.isPresent
                                ]
                                // commented out until CvParam filling is done
                                //testCase $"Person{i + 1}" <| fun () -> Validate.CvBase.person p |> throwError XLSXFile.isValidTerm
                        )
                    )
                    testList "Studies" [
                        for (p,id) in arcConfig.StudyPathsAndIds do
                            if p.IsNone && id.IsSome then
                                let assumedFilename = Path.Combine(pathConfig.StudiesPath, $"{id.Value}\\isa.study.xlsx")
                                let errorMessage = ErrorMessage.FilesystemEntry.createFromFile assumedFilename |> Error
                                // Validate every Study in the Investigation that has no Study filename: (outcome will always be Error)
                                testCase $"{id.Value}" <| fun () -> throwError FilesystemEntry.isPresent errorMessage
                            if p.IsNone && id.IsNone then
                                let errorMessage = ErrorMessage.FilesystemEntry.createFromFile pathConfig.InvestigationPath |> Error
                                // Validate every Study in the Investigation that neither has an Identifier nor a filename: (outcome will always be Error)
                                testCase "(no Study identifier)" <| fun () -> throwError FilesystemEntry.isRegistered errorMessage
                                // commented out until CvParam filling is done
                                //testCase $"Person{i + 1}" <| fun () -> Validate.CvBase.person p |> throwError XLSXFile.isValidTerm
                                //testCase "(no Study identifier)" <| fun () -> throwError XLSXFile.isRegistered investigationPath

                        for (p,id) in arcConfig.StudyFilesAndIds do
                            if p.IsSome then
                                // Validate every Study in the ARC filesystem for registration in the Investigation:
                                testCase $"{id}" <| fun () -> 
                                    Validate.FilesystemEntry.StudyFile.registrationInInvestigation arcConfig.StudyPathsAndIds p.Value |> throwError FilesystemEntry.isRegistered
                    ]
                ]
            ]
        ]