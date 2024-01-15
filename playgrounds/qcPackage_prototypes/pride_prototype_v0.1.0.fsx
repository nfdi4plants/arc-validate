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

invFileTokens |> Seq.iter (Param.getCvName >> printfn "%s")
Investigation.parseMetadataSheetsFromTokens() absoluteFilePaths |> List.concat |> Seq.iter (Param.getCvName >> printfn "%s")
Investigation.parseMetadataSheetsFromTokens() absoluteFilePaths |> List.concat |> Seq.iter (Param.getTerm >> printfn "%A")

let invFileTokensNoMdSecKeys =
    invFileTokens
    |> Seq.filter (Param.getValue >> (<>) Terms.StructuralTerms.metadataSectionKey.Name) 

let contactsFns =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``)

let contactsLns =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``)

let contactsAffs =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``)

let contactsEmails =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``)

let commis =
    invFileTokensNoMdSecKeys
    |> Seq.filter (Param.getTerm >> (=) Terms.StructuralTerms.userComment)


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
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person First Name``.Name) {
            contactsFns
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Last Name``.Name) {
            contactsLns
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``.Name) {
            contactsAffs
            |> Seq.iter Validate.Param.ValueIsNotEmpty
        }
        ARCExpect.validationCase (TestID.Name INVMSO.``Investigation Metadata``. ``INVESTIGATION CONTACTS``.``Investigation Person Email``.Name) {
            contactsEmails
            |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)
        }
        // missing: how to get specific comment? (here: Keywords Comment)
        //ARCExpect.validationCase (TestID.Name "Comment: Keywords") {
        //    commis
        //    |> Seq.iter (Validate.Param.ValueMatchesRegex StringValidationPattern.email)    // needs special Regex
        //}
    ]


// Execution:

Execute.ValidationPipeline(outDirResXml, outDirBadge, "PRIDE") cases