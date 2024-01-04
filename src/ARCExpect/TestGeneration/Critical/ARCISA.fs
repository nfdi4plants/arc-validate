namespace ARCExpect.TestGeneration.Critical.ARC

open ARCExpect
open ControlledVocabulary
open ARCTokenization
open CvParamExtensions
open ARCExpect.Configs

open ARCTokenization
open ARCTokenization.StructuralOntology

module ISA =

    open Expecto
    open FSharpAux

    let generateISATests (tokens: IParam list) =

        let cvParams = tokens |> List.choose Param.tryCvParam
        
        //let pathConfig = arcConfig.PathConfig

        testList INVMSO.``Investigation Metadata``.key.Name [
            testList INVMSO.``Investigation Metadata``.INVESTIGATION.key.Name [
                validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name) {
                    cvParams
                    |> Validate.ByTerm.contains INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
                }
                validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name) {
                    cvParams
                    |> Validate.ByTerm.contains INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``
                }
            ]
        ]