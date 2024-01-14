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


// Input:

let arcDir = @"C:\Repos\git.nfdi4plants.org\ArcPrototype"
let outDir = arcDir
let outDirBadge = Path.Combine(arcDir, "Invenio_badge.svg")
let outDirResXml = Path.Combine(arcDir, "Invenio_results.xml")


// Values:

let absoluteDirectoryPaths = FileSystem.parseAbsoluteDirectoryPaths arcDir
let absoluteFilePaths = FileSystem.parseAbsoluteFilePaths arcDir

let invFileTokens = 
    Investigation.parseMetadataSheetsFromTokens() absoluteFilePaths 
    |> List.concat
    |> ARCGraph.fillTokenList Terms.InvestigationMetadata.ontology
    |> Seq.concat
    |> Seq.concat
    |> Seq.map snd

let invFileTokensNoMdSecKeys =
    invFileTokens
    |> Seq.filter (Param.getValue >> (<>) Terms.StructuralTerms.metadataSectionKey.Name) 

let contactsAffs =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``)

let contactsEmails =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``)


// Validation Cases:

let cases = 
    testList INVMSO.``Investigation Metadata``.INVESTIGATION.key.Name [
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``.Name) {
            invFileTokensNoMdSecKeys
            |> Validate.ParamCollection.ContainsParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``.Name) {
            invFileTokensNoMdSecKeys
            |> Validate.ParamCollection.ContainsParamWithTerm
                INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name) {
            contactsAffs
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name) {
            contactsEmails
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)
        }
    ]


// Execution:

Execute.ValidationPipeline(outDirResXml, outDirBadge, "Invenio") cases