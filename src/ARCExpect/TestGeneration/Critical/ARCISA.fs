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
                ARCExpect.test (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name) {
                    cvParams
                    |> ARCExpect.ByTerm.contains INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
                }
                ARCExpect.test (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name) {
                    cvParams
                    |> ARCExpect.ByTerm.contains INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
                }
            ]
        ]