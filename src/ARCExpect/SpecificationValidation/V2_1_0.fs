namespace ARCExpect.SpecificationValidation


open ControlledVocabulary
open ARCTokenization
open ARCExpect
open Expecto
open AVPRIndex
open System.Text.RegularExpressions
open System.IO
open FSharpAux


module V2_1_0 =

    module Helpers =

        let getAbsoluteDirectoryPaths arcDir = FileSystem.parseARCFileSystem arcDir

        let getInvestigationMetadata arcDir = 
            getAbsoluteDirectoryPaths arcDir
            |> Investigation.parseMetadataSheetsFromTokens() arcDir 

        let getStudyMetadata arcDir = 
            getAbsoluteDirectoryPaths arcDir
            |> Study.parseMetadataSheetsFromTokens() arcDir

        let getStudyFiles arcDir = 
            try 
                getAbsoluteDirectoryPaths arcDir
                |> Study.parseProcessGraphColumnsFromTokens arcDir
            with
                | _ -> seq {Map.empty}

        let getAssayMetadata arcDir =
            getAbsoluteDirectoryPaths arcDir
            |> Assay.parseMetadataSheetsFromTokens() arcDir

        /// The isa.assay.xlsx files of an ARC.
        let getAssayFiles arcDir = 
            try
                getAbsoluteDirectoryPaths arcDir
                |> Assay.parseProcessGraphColumnsFromTokens arcDir
            with
                | _ -> seq {Map.empty}

        /// The filepaths to the files in the dataset folders of each Assay.
        let getAssayDatasetFilePaths arcDir =
            let pattern = Regex(@"^assays/([^/]+)/dataset/([^/]+)$")
            getAbsoluteDirectoryPaths arcDir
            // TO DO: make this its own function in ARCTokenization
            |> Seq.filter (
                fun cvp -> 
                    cvp.Value |> ParamValue.getValueAsString
                    |> pattern.Match
                    |> fun m -> m.Success
            )


        /// The filepaths to the files in the resources folders of each Study.
        let getStudyResourcesFilePaths arcDir =
            let pattern = Regex(@"^studies/([^/]+)/resources/([^/]+)$")
            getAbsoluteDirectoryPaths arcDir
            // TO DO: make this its own function in ARCTokenization
            |> Seq.filter (
                fun cvp -> 
                    cvp.Value |> ParamValue.getValueAsString
                    |> pattern.Match
                    |> fun m -> m.Success
            )

        ///// Returns all paths to the subdirectories in the runs directory in the ARC.
        //let getRunFolderPaths arcDir =
        //    let pattern = Regex(@"^runs/([^/]+)$")  // slash darf nicht! da muss separiert werden weil wenn weiterer slash dann file
        //    getAbsoluteDirectoryPaths arcDir
        //    |> Seq.filter (
            
        //    )

        //let getRunFilePaths arcDir =
        //    let pattern = Regex(@"^studies/([^/]+)/resources/([^/]+)$")
        //    getAbsoluteDirectoryPaths arcDir
        //    |> Seq.filter (
            
        //    )

        /// Takes the path to an ARC and the relative path to a textfile inside this ARC and returns that textfile's content.
        let getTextFileContent arcDir relPathParam =
            Path.Combine(arcDir, relPathParam)
            |> File.ReadAllText
            |> String.replace "\r\n" "\n"   // Windows special character '\013' (= "\r" = Carriage Return) must be purged! (since it causes errors)


        /// <summary>Checks if a string contains the line "cwlVersion v<version>" where version must be: 1.2, 1.2.0-dev1, 1.2.0-dev2, 1.2.0-dev3, 1.2.0-dev4 or 1.2.0-dev5.</summary>
        /// <remarks>Update the Regex pattern as soon as a new CWL version is released.</remarks>
        let isCwlVersionAtLeast1_2 str =
            let pattern = Regex(@"^cwlVersion:\s*v1\.2(?:\.0-dev[1-5])?$")
            (pattern.Match str).Success

        /// Takes a seq of Annotation Tables (from a Study file or an Assay file) in the form of Map<string,IParam list list> (where the string is the name of the worksheet where the Annotation Table is located and the IParam list list is the table where inner list = column and outer list = row) and returns the relative filepaths that can be found in the Annotation Table(s).
        let getAnnotationTableFilePaths (annoTables : Map<string,IParam list list>) =
            annoTables.Values
            |> Seq.collect (
                fun annoTable ->
                    annoTable.Head
            )


    open Helpers

    module CriticalTerms = 
        let investigationTerms = 
            seq {
                StructuralOntology.INVMSO.``Investigation Metadata``.``ONTOLOGY SOURCE REFERENCE``.key
                StructuralOntology.INVMSO.``Investigation Metadata``.``ONTOLOGY SOURCE REFERENCE``.``Term Source Name``
                StructuralOntology.INVMSO.``Investigation Metadata``.``ONTOLOGY SOURCE REFERENCE``.``Term Source File``
                StructuralOntology.INVMSO.``Investigation Metadata``.``ONTOLOGY SOURCE REFERENCE``.``Term Source Version``
                StructuralOntology.INVMSO.``Investigation Metadata``.``ONTOLOGY SOURCE REFERENCE``.``Term Source Description``
                StructuralOntology.INVMSO.``Investigation Metadata``.INVESTIGATION.key
                StructuralOntology.INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Identifier``
                StructuralOntology.INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Title``
                StructuralOntology.INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Description``
                StructuralOntology.INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Submission Date``
                StructuralOntology.INVMSO.``Investigation Metadata``.INVESTIGATION.``Investigation Public Release Date``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION PUBLICATIONS``.key
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION PUBLICATIONS``.``Investigation Publication PubMed ID``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION PUBLICATIONS``.``Investigation Publication DOI``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION PUBLICATIONS``.``Investigation Publication Author List``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION PUBLICATIONS``.``Investigation Publication Title``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION PUBLICATIONS``.``Investigation Publication Status``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION PUBLICATIONS``.``Investigation Publication Status Term Accession Number``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION PUBLICATIONS``.``Investigation Publication Status Term Source REF``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.key
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Last Name``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person First Name``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Mid Initials``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Phone``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Fax``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Address``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Affiliation``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Roles``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Roles Term Accession Number``
                StructuralOntology.INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Roles Term Source REF``
            }

        let studyTerms =
            seq {
                StructuralOntology.STDMSO.``Study Metadata``.STUDY.key
                StructuralOntology.STDMSO.``Study Metadata``.STUDY.``Study Identifier``
                StructuralOntology.STDMSO.``Study Metadata``.STUDY.``Study Title``
                StructuralOntology.STDMSO.``Study Metadata``.STUDY.``Study Description``
                StructuralOntology.STDMSO.``Study Metadata``.STUDY.``Study Submission Date``
                StructuralOntology.STDMSO.``Study Metadata``.STUDY.``Study Public Release Date``
                StructuralOntology.STDMSO.``Study Metadata``.STUDY.``Study File Name``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY DESIGN DESCRIPTORS``.key
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY DESIGN DESCRIPTORS``.``Study Design Type``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY DESIGN DESCRIPTORS``.``Study Design Type Term Accession Number``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY DESIGN DESCRIPTORS``.``Study Design Type Term Source REF``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY PUBLICATIONS``.key
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY PUBLICATIONS``.``Study Publication PubMed ID``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY PUBLICATIONS``.``Study Publication DOI``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY PUBLICATIONS``.``Study Publication Author List``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY PUBLICATIONS``.``Study Publication Title``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY PUBLICATIONS``.``Study Publication Status``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY PUBLICATIONS``.``Study Publication Status Term Accession Number``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY PUBLICATIONS``.``Study Publication Status Term Source REF``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.key
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person Last Name``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person First Name``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person Mid Initials``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person Email``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person Phone``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person Fax``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person Address``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person Affiliation``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person Roles``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person Roles Term Accession Number``
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY CONTACTS``.``Study Person Roles Term Source REF``
            }

        let assayTerms = 
            seq {
                StructuralOntology.ASSMSO.``Assay Metadata``.ASSAY.key
                StructuralOntology.ASSMSO.``Assay Metadata``.ASSAY.``Assay Measurement Type``
                StructuralOntology.ASSMSO.``Assay Metadata``.ASSAY.``Assay Measurement Type Term Accession Number``
                StructuralOntology.ASSMSO.``Assay Metadata``.ASSAY.``Assay Measurement Type Term Source REF``
                StructuralOntology.ASSMSO.``Assay Metadata``.ASSAY.``Assay Technology Type``
                StructuralOntology.ASSMSO.``Assay Metadata``.ASSAY.``Assay Technology Type Term Accession Number``
                StructuralOntology.ASSMSO.``Assay Metadata``.ASSAY.``Assay Technology Type Term Source REF``
                StructuralOntology.ASSMSO.``Assay Metadata``.ASSAY.``Assay Technology Platform``
                StructuralOntology.ASSMSO.``Assay Metadata``.ASSAY.``Assay File Name``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.key
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person Last Name``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person First Name``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person Mid Initials``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person Email``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person Phone``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person Fax``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person Address``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person Affiliation``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person Roles``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person Roles Term Accession Number``
                StructuralOntology.ASSMSO.``Assay Metadata``.``ASSAY PERFORMERS``.``Assay Person Roles Term Source REF``
            }


    module NonCriticalTerms =
        let investigationTerms = 
            seq {
                StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.key,
                    seq {
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study Identifier``
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study Description``
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study Title``
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study Submission Date``
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study Public Release Date``
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study File Name``
                    }
                
                StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY DESIGN DESCRIPTORS``.key,
                    seq {
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY DESIGN DESCRIPTORS``.``Study Design Type``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY DESIGN DESCRIPTORS``.``Study Design Type Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY DESIGN DESCRIPTORS``.``Study Design Type Term Source REF``
                    }

                StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.key,
                    seq {
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication PubMed ID``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication DOI``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication Author List``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication Title``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication Status``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication Status Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication Status Term Source REF``
                    }

                StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY FACTORS``.key,
                    seq {
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY FACTORS``.``Study Factor Name``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY FACTORS``.``Study Factor Type``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY FACTORS``.``Study Factor Type Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY FACTORS``.``Study Factor Type Term Source REF``
                    }

                StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY ASSAYS``.key,
                    seq {
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY ASSAYS``.``Study Assay Measurement Type``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY ASSAYS``.``Study Assay Measurement Type Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY ASSAYS``.``Study Assay Measurement Type Term Source REF``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY ASSAYS``.``Study Assay Technology Type``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY ASSAYS``.``Study Assay Technology Type Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY ASSAYS``.``Study Assay Technology Type Term Source REF``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY ASSAYS``.``Study Assay Technology Platform``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY ASSAYS``.``Study Assay File Name``
                    }

                StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.key,
                    seq {
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Name``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Type``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Type Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Type Term Source REF``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Description``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol URI``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Version``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Parameters Name``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Parameters Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Parameters Term Source REF``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Components Name``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Components Type``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Components Type Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PROTOCOLS``.``Study Protocol Components Type Term Source REF``           
                    }

                StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.key,
                    seq {
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person Last Name``
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person First Name``
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person Mid Initials``
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person Email``
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person Phone``
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person Fax``
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person Address``
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person Affiliation``
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person Roles``
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person Roles Term Accession Number``
                    StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY CONTACTS``.``Study Person Roles Term Source REF``
                    }
            }

        let studyTerms =
            seq {
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY FACTORS``.key,
                seq {
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY FACTORS``.``Study Factor Name``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY FACTORS``.``Study Factor Type``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY FACTORS``.``Study Factor Type Term Accession Number``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY FACTORS``.``Study Factor Type Term Source REF``
                }

                StructuralOntology.STDMSO.``Study Metadata``.``STUDY ASSAYS``.key,
                seq {
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY ASSAYS``.``Study Assay Measurement Type``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY ASSAYS``.``Study Assay Measurement Type Term Accession Number``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY ASSAYS``.``Study Assay Measurement Type Term Source REF``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY ASSAYS``.``Study Assay Technology Type``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY ASSAYS``.``Study Assay Technology Type Term Accession Number``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY ASSAYS``.``Study Assay Technology Type Term Source REF``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY ASSAYS``.``Study Assay Technology Platform``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY ASSAYS``.``Study Assay File Name``
                }

                StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.key,
                seq {
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Name``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Type``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Type Term Accession Number``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Type Term Source REF``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Description``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol URI``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Version``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Parameters Name``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Parameters Term Accession Number``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Parameters Term Source REF``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Components Name``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Components Type``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Components Type Term Accession Number``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY PROTOCOLS``.``Study Protocol Components Type Term Source REF``           
                }
            }


/// Creates a validation package for this ARC specification.
/// 
/// This code performs a series of validation checks on an ARC (file system structure) to ensure that it meets certain requirements. The checks include:
/// - Checking for the presence of an Investigation file in the ARC.
/// - Checking for the presence of specific directories (Studies, Assays, Runs, Workflows) in the ARC.
/// - Checking if each Study directory features a Study file.
/// - Checking if each Assay directory features an Assay file.
/// - Checking if each Workflow directory features a CWL file in v1.2 or higher.
/// - Checking if each Run directory features a CWL file in v1.2 or higher.
/// - Checking if every file linked in the Investigation is present in the ARC.
/// - Checking if every file linked in the Studies is present in the ARC.
/// - Checking if the progress graph in Studies and Assays contains free text.
/// - Checking if all required Investigation metadata fields are present.
/// - Checking if all required Study metadata fields are present.
/// - Checking if all required Assay metadata fields are present.
///
/// The code uses the ARCExpect module and various functions from the Validate module to perform these checks.
    let getValidationCases (arcDir : string) =

        let absoluteDirectoryPaths  = getAbsoluteDirectoryPaths arcDir
        let investigationMetadata   = getInvestigationMetadata arcDir
        let studyMetadata           = getStudyMetadata arcDir
        let studyFiles              = getStudyFiles arcDir
        let assayMetadata           = getAssayMetadata arcDir
        let assayFiles              = getAssayFiles arcDir

        let criticalCases = 
            [ 
                // Check for Investigation
                ARCExpect.validationCase (TestID.Name "ARC contains Investigation file") {
                    absoluteDirectoryPaths
                    |> Validate.ParamCollection.ContainsParamWithTerm (StructuralOntology.AFSO.``Investigation File``)
                }

                // Check for folder structure
                ARCExpect.validationCase (TestID.Name "ARC contains Studies directory") {
                    absoluteDirectoryPaths
                    |> Validate.ParamCollection.ContainsParamWithTerm (StructuralOntology.AFSO.``Studies Directory``)
                }
                ARCExpect.validationCase (TestID.Name "ARC contains Assays directory") {
                    absoluteDirectoryPaths
                    |> Validate.ParamCollection.ContainsParamWithTerm (StructuralOntology.AFSO.``Assays Directory``)
                }
                ARCExpect.validationCase (TestID.Name "ARC contains Runs directory") {
                    absoluteDirectoryPaths
                    |> Validate.ParamCollection.ContainsParamWithTerm (StructuralOntology.AFSO.``Runs Directory``)
                }
                ARCExpect.validationCase (TestID.Name "ARC contains Workflows directory") {
                    absoluteDirectoryPaths
                    |> Validate.ParamCollection.ContainsParamWithTerm (StructuralOntology.AFSO.``Workflows Directory``)
                }

                //Check if each Study directory features a Study file
                ARCExpect.validationCase (TestID.Name "ARC contains Study in Studies directory") {
                    let studyDirs = 
                        absoluteDirectoryPaths 
                        |> Seq.filter (fun x -> x.Name = StructuralOntology.AFSO.``Study Directory``.Name)
                        |> Seq.map (fun x -> x.Value |> ParamValue.getValue |> string)
                    let studies = 
                        absoluteDirectoryPaths 
                        |> Seq.filter (fun x -> x.Name = StructuralOntology.AFSO.``Study File``.Name)

                    studies
                    |> Validate.ParamCollection.SatisfiesPredicate (fun x -> Seq.length x = Seq.length studyDirs) 

                    studies
                    |> Validate.ParamCollection.SatisfiesPredicate (fun x -> 
                        x
                        |> Seq.forall (
                            fun y -> 
                                studyDirs
                                |> Seq.contains (
                                    y.Value
                                    |> ParamValue.getValueAsString
                                    |> fun x -> x.Split('/')
                                    |> Array.rev
                                    |> Array.tail
                                    |> Array.rev
                                    |> String.concat "/"
                                ) 
                        ) 
                    ) 
                }

                // Check if each Assay directory features an Assay file
                ARCExpect.validationCase (TestID.Name "ARC contains Assay in Assay directory") {
                    let assayDir = 
                        absoluteDirectoryPaths 
                        |> Seq.filter (fun x -> x.Name = StructuralOntology.AFSO.``Assay Directory``.Name)
                        |> Seq.map (fun x -> x.Value |> ParamValue.getValue |> string)
                    let assays = 
                        absoluteDirectoryPaths 
                        |> Seq.filter (fun x -> x.Name = StructuralOntology.AFSO.``Assay File``.Name)

                    assays
                    |> Validate.ParamCollection.SatisfiesPredicate (fun x -> Seq.length x = Seq.length assayDir) 

                    assays
                    |> Validate.ParamCollection.SatisfiesPredicate (
                        fun x -> 
                            x
                            |> Seq.forall (
                                fun y -> 
                                    assayDir
                                    |> Seq.contains (
                                        y.Value
                                        |> ParamValue.getValueAsString
                                        |> fun x -> x.Split('/')
                                        |> Array.rev
                                        |> Array.tail
                                        |> Array.rev
                                        |> String.concat"/"
                                    ) 
                            ) 
                    ) 
                }

                // Check if each Workflow directory features a CWL file in v1.2+
                ARCExpect.validationCase (TestID.Name "ARC contains Workflow file in every Workflow directory") {
                    let workflowDir = 
                        absoluteDirectoryPaths 
                        |> Seq.filter (fun x -> x.Name = StructuralOntology.AFSO.``Workflow Directory``.Name)
                        |> Seq.map (fun x -> x.Value |> ParamValue.getValue |> string)

                    let cwls = 
                        absoluteDirectoryPaths 
                        |> Seq.filter (
                            fun x -> 
                                x.Name = StructuralOntology.AFSO.``CWL File``.Name && 
                                x.Value
                                |> ParamValue.getValueAsString
                                |> fun (x : string) -> x.Split [|'/'|]
                                |> fun x -> 
                                    match x with 
                                    | [|"workflows";_;_|] -> true
                                    | _ -> false
                        )

                    //cwls
                    //|> Validate.ParamCollection.SatisfiesPredicate (fun x -> (Seq.length x) = (workflowDir|>Seq.length)) 

                    // Check for CWL presence in every subdir
                    cwls
                    |> Validate.ParamCollection.SatisfiesPredicate (
                        fun x -> 
                            x
                            |> Seq.forall (
                                fun y -> 
                                    workflowDir
                                    |> Seq.contains (
                                        y.Value
                                        |> ParamValue.getValueAsString
                                        |> fun x -> 
                                            x.Split('/')
                                            |> Array.rev
                                            |> Array.tail
                                            |> Array.rev
                                            |> String.concat "/"
                                    ) 
                            ) 
                    ) 

                    // Check for allowed CWL version
                    cwls
                    |> Validate.ParamCollection.SatisfiesPredicate (
                        fun x ->
                            x
                            |> Seq.forall (
                                Param.getValueAsString
                                >> getTextFileContent arcDir
                                >> isCwlVersionAtLeast1_2
                            )
                    )
                }

                // Check if each Workflow directory features a CWL file in v1.2+
                ARCExpect.validationCase (TestID.Name "ARC contains Run file in every Run directory") {
                    let runDir = 
                        absoluteDirectoryPaths 
                        |> Seq.choose (
                            fun absoluteDirectoryPath ->
                                if Param.getCvName absoluteDirectoryPath = StructuralOntology.AFSO.``Run Directory``.Name then
                                    Some (Param.getValueAsString absoluteDirectoryPath)
                                else None
                        )

                    let cwls = 
                        absoluteDirectoryPaths 
                        |> Seq.filter (
                            fun x -> 
                                x.Name = StructuralOntology.AFSO.``CWL File``.Name && 
                                x.Value
                                |> ParamValue.getValueAsString
                                |> fun (x : string) -> x.Split [|'/'|]
                                |> fun x -> 
                                    match x with 
                                    | [|"runs";_;_|] -> true
                                    | _ -> false
                        )

                    // Check for CWL presence in every subdir
                    cwls
                    |> Validate.ParamCollection.SatisfiesPredicate (
                        fun x -> 
                            x
                            |> Seq.forall (
                                fun y -> 
                                    runDir
                                    |> Seq.contains (
                                        y.Value
                                        |> ParamValue.getValueAsString
                                        |> fun x -> 
                                            x.Split('/')
                                            |> Array.rev
                                            |> Array.tail
                                            |> Array.rev
                                            |> String.concat "/"
                                    ) 
                            ) 
                    ) 

                    // Check for allowed CWL version
                    cwls
                    |> Validate.ParamCollection.SatisfiesPredicate (
                        fun x ->
                            x
                            |> Seq.forall (
                                Param.getValueAsString
                                >> getTextFileContent arcDir
                                >> isCwlVersionAtLeast1_2
                            )
                    )
                }

                // TO DO: missing: Check if output files from run.cwl can be found in the Run's subdir
                // difficult due to complex CWL syntax – parser needed (CWLDotNet might be insufficient)

                // Check if every file linked in Investigation is present
                ARCExpect.validationCase (TestID.Name "ARC contains all files linked in Investigation") {
                    let investigationFiles : IParam seq  = 
                        investigationMetadata
                        |> Seq.concat
                        |> Seq.choose (
                            fun x -> 
                                match x.Name with 
                                | "Study File Name"         -> 
                                    match x.Value.GetType().Name with
                                    | "CvValue" -> None
                                    | _         -> Some x
                            
                                | "Study Assay File Name"   -> 
                                    match x.Value.GetType().Name with
                                    | "CvValue" -> None
                                    | _         -> Some x
                                | _ -> None
                        )

                    investigationFiles
                    |> Seq.iter (
                        fun (x : IParam) -> 
                            Validate.ParamCollection.ContainsParamWithValue (
                                x
                                |> Param.getValue
                                |> string
                                |> fun x -> x.Replace("\\","/")
                            ) absoluteDirectoryPaths
                    )
                }

                // Check if every file linked in Studies is present
                ARCExpect.validationCase (TestID.Name "ARC contains all files linked in Studies") {
                    let studyFiles: IParam seq  = 
                        studyMetadata
                        |> Seq.concat
                        |> Seq.choose (
                            fun x -> 
                                match x.Name with 
                                | "Study Assay File Name"   -> 
                                    match x.Value.GetType().Name with
                                    | "CvValue" -> None
                                    | _         -> Some x
                                | _ -> None
                        )

                    studyFiles
                    |> Seq.iter (
                        fun (x : IParam) -> 
                            Validate.ParamCollection.ContainsParamWithValue (
                                x
                                |> Param.getValue
                                |> string
                                |> fun x -> x.Replace("\\","/")
                            ) absoluteDirectoryPaths
                    )
                }        

                // Check if progress graph contains free text for each study
                ARCExpect.validationCase (TestID.Name "ARC is free of falsely parsed process graph headers in Studies") {
                    let graphHeaders = 
                        studyFiles
                        |> Seq.concat
                        |> Seq.map (
                            fun x -> 
                                x.Value
                                |> List.map List.head
                        )
                        |> Seq.concat

                    graphHeaders
                    |> Validate.ParamCollection.SatisfiesPredicate (
                        fun x -> 
                            x
                            |> Seq.map Param.getTerm
                            |> Seq.contains StructuralOntology.APGSO.FreeText
                            |> not
                    )
                }

                // Check if progress graph contains free text for each Study
                ARCExpect.validationCase (TestID.Name "ARC is free of falsely parsed process graph headers in Assays") {
                    let graphHeaders = 
                        assayFiles
                        |> Seq.concat
                        |> Seq.map (
                            fun x -> 
                                x.Value
                                |> List.map List.head
                        )
                        |> Seq.concat

                    graphHeaders
                    |> Validate.ParamCollection.SatisfiesPredicate (
                        fun x -> 
                            x
                            |> Seq.map Param.getTerm
                            |> Seq.contains StructuralOntology.APGSO.FreeText
                            |> not
                    )

                }

                // Check if every required Investigation metadata field is there
                ARCExpect.validationCase (TestID.Name "ARC contains all required Investigation metadata fields") {

                    let investigations = List.concat investigationMetadata
                    CriticalTerms.investigationTerms
                    |> Seq.iter (fun key -> Validate.ParamCollection.ContainsParamWithTerm key investigations)
                }

                // Check if every required Study metadata field is there
                ARCExpect.validationCase (TestID.Name "ARC contains all required Study metadata fields") {

                    for studySingular in studyMetadata do
                        CriticalTerms.studyTerms
                        |> Seq.iter (fun key -> Validate.ParamCollection.ContainsParamWithTerm key studySingular)
                }

                // Check if every required Assay metadata field is there
                ARCExpect.validationCase (TestID.Name "ARC contains all required Assay metadata fields") {

                    for assaySingular in assayMetadata do
                        CriticalTerms.assayTerms
                        |> Seq.iter (fun key -> Validate.ParamCollection.ContainsParamWithTerm key assaySingular)
                }

                // Check
        ]

        let nonCriticalCases = [
            // Check if Investigation metadata contains optional fields
            ARCExpect.validationCase (TestID.Name "ARC contains Investigation optional Metadata fields") {

                let investigations = List.concat investigationMetadata

                NonCriticalTerms.investigationTerms
                |> Seq.iter (
                    fun (key,valSeq) -> 
                        let condition = investigations |> Seq.exists (Param.getCvAccession >> (=) key.Accession)
                        if condition then
                            valSeq
                            |> Seq.iter (fun x -> Validate.ParamCollection.ContainsParamWithTerm x investigations)
                )
            }

            // Check if Study metadata contains optional fields
            ARCExpect.validationCase (TestID.Name "ARC contains Study optional Metadata fields") {

                for studySingular in studyMetadata do
                    NonCriticalTerms.studyTerms
                    |> Seq.iter (
                        fun (key,valSeq) -> 
                            let condition = studySingular |> Seq.exists (Param.getCvAccession >> (=) key.Accession)
                            if condition then
                                valSeq
                                |> Seq.iter (fun x -> Validate.ParamCollection.ContainsParamWithTerm x studySingular)
                    )
            }
        ]

        ARCValidationPackage.create(
            metadata = ValidationPackageMetadata.create(
                name = "arc_specification",
                summary = "Validate whether an ARC conforms to Specification V2.1.0",
                description = "Validate whether an ARC conforms to Specification V2.1.0. See the relevant spec at https://github.com/nfdi4plants/ARC-specification/blob/3435ce03c0d2697f01e3a4607c8f15c3b195c97d/ARC%20specification.md",
                majorVersion = 2,
                minorVersion = 1,
                patchVersion = 0
            ),
            CriticalValidationCasesList = criticalCases
        )