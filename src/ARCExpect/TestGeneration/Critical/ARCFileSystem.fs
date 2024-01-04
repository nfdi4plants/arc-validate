namespace ARCExpect.TestGeneration.Critical.ARC

open ARCExpect
open ControlledVocabulary
open ARCTokenization
open CvParamExtensions
open ARCExpect.Configs

open ARCTokenization
open ARCTokenization.StructuralOntology

module FileSystem =

    open Expecto
    open FSharpAux

    let generateARCFileSystemTests (rootPath: string) =
        
        let relativeDirectoryPaths = FileSystem.parseRelativeDirectoryPaths rootPath
        let relativeFilePaths = FileSystem.parseRelativeFilePaths rootPath

        //let pathConfig = arcConfig.PathConfig

        // we need a structuralontology here as well (e.g. terms for 'Investigation File', 'Study File')
        testList "FileSystem" [
            testList "Investigation" [
                validationCase (TestID.Name "Investigation File") {
                    relativeFilePaths |> Validate.ByValue.contains "isa.investigation.xlsx"
                }
            ]
        ]