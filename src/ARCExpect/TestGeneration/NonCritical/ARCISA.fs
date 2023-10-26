namespace ARCExpect.TestGeneration.NonCritical.ARC

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
    //open System.IO

    let generateISATests (tokens: IParam list) =

        let cvParams = tokens |> List.choose Param.tryCvParam
        
        //let pathConfig = arcConfig.PathConfig

        testList INVMSO.``Investigation Metadata``.key.Name [

        ]