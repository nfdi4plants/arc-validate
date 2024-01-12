#I "../../src/arc-validate/bin/Debug/net8.0"
#I "../../src/arc-validate/bin/Release/net8.0"
#r "ARCExpect.dll"
#r "ARCTokenization.dll"
#r "ControlledVocabulary.dll"
#r "arc-validate.dll"
#r "Anybadge.NET.dll"
#r "Expecto.dll"
#r "OBO.NET.dll"

//#r "nuget: ARCExpect"
#r "nuget: Anybadge.NET"
#r "nuget: FSharpAux"
#r "nuget: Graphoscope"
#r "nuget: Cytoscape.NET"

open ARCExpect
open ARCTokenization
open ARCTokenization.StructuralOntology
open ControlledVocabulary
open Expecto
open Expecto.Tests
open AnyBadge.NET
open ARCValidate
open FSharpAux
open Graphoscope
open Cytoscape

open System.IO


// input:

let arcDir = @"C:\Repos\git.nfdi4plants.org\ArcPrototype"
let outDir = arcDir


// values:

let absoluteDirectoryPaths = FileSystem.parseRelativeDirectoryPaths arcDir
let absoluteFilePaths = FileSystem.parseRelativeFilePaths arcDir

let invFileTokens = 
    Investigation.parseMetadataSheetsFromTokens() absoluteFilePaths 
    |> List.concat
    |> ARCGraph.fillTokenList Terms.InvestigationMetadata.ontology
    |> Seq.concat
    |> Seq.concat
    |> Seq.map snd

let invFileTokensNoMdSecKeys =
    invFileTokens
    |> Seq.filter (Param.getTerm >> (<>) Terms.StructuralTerms.metadataSectionKey) 


// Validation Cases:

let cases = [
    testList INVMSO.``Investigation Metadata``.INVESTIGATION.key.Name [
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name) {
            invFileTokensNoMdSecKeys
            |> Validate.ByTerm.contains
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name) {
            invFileTokensNoMdSecKeys
            |> Validate.ByTerm.contains
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``
        }
    ]
]