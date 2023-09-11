namespace ARCExpect.TestGeneration.NonCritical.ARC

open ARCExpect
open ARCExpect.Configs

module ISA =

    open Expecto
    open FSharpAux
    //open System.IO

    let generateISATests (arcConfig: ARCConfig) =

        //let pathConfig = arcConfig.PathConfig

        testList "ISA" [
            testList "Semantic" [
                testList "Investigation" [
                    testList "Person" []
                ]
            ]
        ]