namespace ArcValidation.TestGeneration.NonCritical.Arc

open ArcValidation
open ArcValidation.Configs

module ISA =

    open Expecto
    open FSharpAux
    //open System.IO

    let generateISATests (arcConfig: ArcConfig) =

        //let pathConfig = arcConfig.PathConfig

        testList "ISA" [
            testList "Semantic" [
                testList "Investigation" [
                    testList "Person" []
                ]
            ]
        ]