namespace ArcValidation.TestGeneration.Critical.Arc

open ArcValidation
open ArcValidation.Configs

module FileSystem =

    open Expecto

    let generateArcFileSystemTests (arcConfig: ArcConfig) =

        let pathConfig = arcConfig.PathConfig

        testList "Filesystem" [
            testList "Git" [
            ]
            testList "Studies" [
            ]
            testList "Assays" [
            ]
            testList "DataPathNames" [
            ]
        ]