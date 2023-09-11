namespace ARCExpect.TestGeneration.Critical.ARC

open ARCExpect
open ARCExpect.Configs

module FileSystem =

    open Expecto

    let generateARCFileSystemTests (arcConfig: ARCConfig) =

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