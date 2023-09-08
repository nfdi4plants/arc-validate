namespace ArcValidation.TestGeneration.Critical.Arc

open ArcValidation
open ArcValidation.Configs

module ISA =

    open Expecto
    open FSharpAux

    let generateISATests (arcConfig : ArcConfig) =

        let pathConfig = arcConfig.PathConfig

        testList "ISA" [
            testList "Semantic" [
                testList "Investigation" [
                    testList "Studies" [
                    ]
                ]
            ]
        ]