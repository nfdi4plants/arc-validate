namespace ARCExpect.SpecificationValidation

open ControlledVocabulary
open ARCTokenization
open ARCExpect
open Expecto
open AVPRIndex

module V2_0_0_Draft =

    module MustHaveTerms = 
        let investigationTerms = 
            seq{
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
            seq{
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
            seq{
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

    module MayHave =
        let investigationTerms = 
            seq{
                StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.key,
                    seq{
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study Identifier``
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study Description``
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study Title``
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study Submission Date``
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study Public Release Date``
                        StructuralOntology.INVMSO.``Investigation Metadata``.STUDY.``Study File Name``
                    }
                
                StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY DESIGN DESCRIPTORS``.key,
                    seq{
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY DESIGN DESCRIPTORS``.``Study Design Type``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY DESIGN DESCRIPTORS``.``Study Design Type Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY DESIGN DESCRIPTORS``.``Study Design Type Term Source REF``
                    }
                StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.key,
                    seq{
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication PubMed ID``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication DOI``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication Author List``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication Title``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication Status``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication Status Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY PUBLICATIONS``.``Study Publication Status Term Source REF``
                    }
                StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY FACTORS``.key,
                    seq{
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY FACTORS``.``Study Factor Name``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY FACTORS``.``Study Factor Type``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY FACTORS``.``Study Factor Type Term Accession Number``
                        StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY FACTORS``.``Study Factor Type Term Source REF``
                    }
                StructuralOntology.INVMSO.``Investigation Metadata``.``STUDY ASSAYS``.key,
                    seq{
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
                    seq{
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
                    seq{
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
            seq{
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY FACTORS``.key,
                seq{
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY FACTORS``.``Study Factor Name``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY FACTORS``.``Study Factor Type``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY FACTORS``.``Study Factor Type Term Accession Number``
                    StructuralOntology.STDMSO.``Study Metadata``.``STUDY FACTORS``.``Study Factor Type Term Source REF``
                }
                StructuralOntology.STDMSO.``Study Metadata``.``STUDY ASSAYS``.key,
                seq{
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
                seq{
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

    let validationCases (arcDir:string) =

        let absoluteDirectoryPaths = FileSystem.parseARCFileSystem arcDir

        let investigationMetadata = 
            absoluteDirectoryPaths
            |> Investigation.parseMetadataSheetsFromTokens() arcDir 

        let studyMetadata = 
            absoluteDirectoryPaths
            |> Study.parseMetadataSheetsFromTokens() arcDir

        let studyFiles = 
            try 
                absoluteDirectoryPaths
                |> Study.parseProcessGraphColumnsFromTokens arcDir
            with
                | _ -> seq{Map.empty}

        let assayMetadata =
            absoluteDirectoryPaths
            |> Assay.parseMetadataSheetsFromTokens() arcDir
        
        let assayFiles = 
            try
                absoluteDirectoryPaths
                |> Assay.parseProcessGraphColumnsFromTokens arcDir
            with
                | _ -> seq{Map.empty}


        /// This code performs a series of validation checks on an Arc (file system structure) to ensure that it meets certain requirements. The checks include:
        /// - Checking for the presence of an investigation file in the Arc.
        /// - Checking for the presence of specific directories (Studies, Assays, Runs, Workflows) in the Arc.
        /// - Checking if each study directory features a study file.
        /// - Checking if each assay directory features an assay file.
        /// - Checking if each workflow directory features a CWL file.
        /// - Checking if every file linked in the investigation is present in the Arc.
        /// - Checking if every file linked in the studies is present in the Arc.
        /// - Checking if the progress graph in studies and assays contains free text.
        /// - Checking if all needed Investigation Metadata fields are present.
        /// - Checking if all needed Study Metadata fields are present.
        /// - Checking if all needed Assay Metadata fields are present.
        ///
        /// The code uses the ARCExpect module and various functions from the Validate module to perform these checks.
        let cases = 
            [ 

                //Check for investigation
                ARCExpect.validationCase (TestID.Name "Arc contains investigation file") {
                    absoluteDirectoryPaths
                    |> Validate.ParamCollection.ContainsParamWithTerm (StructuralOntology.AFSO.``Investigation File``)
                }

                //Check for folder structure
                ARCExpect.validationCase (TestID.Name "Arc contains Studies Directory") {
                    absoluteDirectoryPaths
                    |> Validate.ParamCollection.ContainsParamWithTerm (StructuralOntology.AFSO.``Studies Directory``)
                }
                ARCExpect.validationCase (TestID.Name "Arc contains Assays Directory") {
                    absoluteDirectoryPaths
                    |> Validate.ParamCollection.ContainsParamWithTerm (StructuralOntology.AFSO.``Assays Directory``)
                }
                ARCExpect.validationCase (TestID.Name "Arc contains Runs Directory") {
                    absoluteDirectoryPaths
                    |> Validate.ParamCollection.ContainsParamWithTerm (StructuralOntology.AFSO.``Runs Directory``)
                }
                ARCExpect.validationCase (TestID.Name "Arc contains Workflows Directory") {
                    absoluteDirectoryPaths
                    |> Validate.ParamCollection.ContainsParamWithTerm (StructuralOntology.AFSO.``Workflows Directory``)
                }

                //Check if each study directory features a study file
                ARCExpect.validationCase (TestID.Name "Arc contains study in studies directory") {
                    let studyDirs = 
                        absoluteDirectoryPaths 
                        |> Seq.filter(fun x -> x.Name = StructuralOntology.AFSO.``Study Directory``.Name)
                        |> Seq.map(fun x -> x.Value|>ParamValue.getValue|>string)
                    let studies = 
                        absoluteDirectoryPaths 
                        |> Seq.filter(fun x -> x.Name = StructuralOntology.AFSO.``Study File``.Name)
                    
                    studies
                    |> Validate.ParamCollection.SatisfiesPredicate (fun x -> (Seq.length x) = (studyDirs|>Seq.length)) 
                    
                    studies
                    |> Validate.ParamCollection.SatisfiesPredicate(fun x -> 
                        x
                        |> Seq.forall (fun y -> 
                            studyDirs
                            |> Seq.contains (
                                y.Value
                                |>ParamValue.getValue
                                |>string
                                |>fun x -> x.Split('/')
                                |> Array.rev
                                |>Array.tail
                                |>Array.rev
                                |>String.concat"/"
                            ) 
                        ) 
                    ) 
                }

                //Check if each assay directory features a assay file
                ARCExpect.validationCase (TestID.Name "Arc contains assay in assay directory") {
                    let assayDir = 
                        absoluteDirectoryPaths 
                        |> Seq.filter(fun x -> x.Name = StructuralOntology.AFSO.``Assay Directory``.Name)
                        |> Seq.map(fun x -> x.Value|>ParamValue.getValue|>string)
                    let assays = 
                        absoluteDirectoryPaths 
                        |> Seq.filter(fun x -> x.Name = StructuralOntology.AFSO.``Assay File``.Name)
                    
                    assays
                    |> Validate.ParamCollection.SatisfiesPredicate (fun x -> (Seq.length x) = (assayDir|>Seq.length)) 
                    
                    assays
                    |> Validate.ParamCollection.SatisfiesPredicate(fun x -> 
                        x
                        |> Seq.forall (fun y -> 
                            assayDir
                            |> Seq.contains (
                                y.Value
                                |>ParamValue.getValue
                                |>string
                                |>fun x -> x.Split('/')
                                |> Array.rev
                                |>Array.tail
                                |>Array.rev
                                |>String.concat"/"
                                ) 
                            ) 
                    ) 
                }

                // Check if each workflow directory features a cwl file
                ARCExpect.validationCase (TestID.Name "Arc contains workflow in workflow directory") {
                    let workflowDir = 
                        absoluteDirectoryPaths 
                        |> Seq.filter(fun x -> x.Name = StructuralOntology.AFSO.``Workflow Directory``.Name)
                        |>Seq.map(fun x -> x.Value|>ParamValue.getValue|>string)

                    let cwls = 
                        absoluteDirectoryPaths 
                        |> Seq.filter(fun x -> 
                            x.Name = StructuralOntology.AFSO.``CWL File``.Name && 
                            x.Value
                            |>ParamValue.getValue
                            |>string
                            |>fun (x: string) -> x.Split [|'/'|]
                            |>fun x -> 
                                match x with 
                                | [|"workflows";_;_|] -> true
                                | _ -> false
                        )
                    
                    cwls
                    |> Validate.ParamCollection.SatisfiesPredicate (fun x -> (Seq.length x) = (workflowDir|>Seq.length)) 
                    
                    cwls
                    |> Validate.ParamCollection.SatisfiesPredicate(fun x -> 
                        x
                        |> Seq.forall (fun y -> 
                            workflowDir
                            |> Seq.contains (
                                y.Value
                                |>ParamValue.getValue
                                |>string
                                |>fun x -> 
                                    x.Split('/')
                                    |> Array.rev
                                    |>Array.tail
                                    |>Array.rev
                                    |>String.concat"/"
                                ) 
                            ) 
                    ) 
                }
            
                //Check if every file linked in investigation is present
                ARCExpect.validationCase (TestID.Name "Arc contains all files linked in investigation") {
                    let investigationFiles: IParam seq  = 
                        investigationMetadata
                        |>Seq.concat
                        |>Seq.choose(fun x -> 
                            match x.Name with 
                            | "Study File Name"         -> 
                                match x.Value.GetType().Name with
                                | "CvValue" -> None
                                | _         -> Some(x)
                            
                            | "Study Assay File Name"   -> 
                                match x.Value.GetType().Name with
                                | "CvValue" -> None
                                | _         -> Some(x)
                            | _ -> None
                        )

                    investigationFiles
                    |> Seq.iter(fun (x: IParam) -> 
                        Validate.ParamCollection.ContainsParamWithValue (x|>Param.getValue|>string|>fun x -> x.Replace("\\","/")) absoluteDirectoryPaths
                    )
                }
                //Check if every file linked in studies is present
                ARCExpect.validationCase (TestID.Name "Arc contains all files linked in studies") {
                    let studyFiles: IParam seq  = 
                        studyMetadata
                        |>Seq.concat
                        |>Seq.choose(fun x -> 
                            match x.Name with 
                            | "Study Assay File Name"   -> 
                                match x.Value.GetType().Name with
                                | "CvValue" -> None
                                | _         -> Some(x)
                            | _ -> None
                        )

                    studyFiles
                    |> Seq.iter(fun (x: IParam) -> 
                        Validate.ParamCollection.ContainsParamWithValue (x|>Param.getValue|>string|>fun x -> x.Replace("\\","/")) absoluteDirectoryPaths
                    )
                }        
                
                // Check if progress graph contains free text fpr each study
                ARCExpect.validationCase (TestID.Name "Arc is free of falesly parsed process graph headers in studies") {
                    let graphHeaders = 
                        studyFiles
                        |> Seq.concat
                        |> Seq.map(fun x -> 
                            x.Value
                            |>List.map(fun y -> 
                                y|>List.head
                            )
                        )
                        |>Seq.concat
                    
                    graphHeaders
                    |> Validate.ParamCollection.SatisfiesPredicate(fun (x) -> 
                        x
                        |> Seq.map(fun y ->
                            Param.getTerm y
                        )
                        |> Seq.contains (StructuralOntology.APGSO.FreeText)|>not
                    )
                }

                // Check if progress graph contains free text fpr each study
                ARCExpect.validationCase (TestID.Name "Arc is free of falesly parsed process graph headers in assays") {
                    let graphHeaders = 
                        assayFiles
                        |> Seq.concat
                        |> Seq.map(fun x -> 
                            x.Value
                            |>List.map(fun y -> 
                                y|>List.head
                            )
                        )
                        |>Seq.concat
                    
                    graphHeaders
                    |> Validate.ParamCollection.SatisfiesPredicate(fun (x) -> 
                        x
                        |> Seq.map(fun y ->
                            Param.getTerm y
                        )
                        |> Seq.contains (StructuralOntology.APGSO.FreeText)|>not
                    )

                }
                //Check if every needed Investigation Metadata field is there
                ARCExpect.validationCase (TestID.Name "Arc contains all needed Investigation Metadata fields") {

                    let investigations = investigationMetadata|>List.concat
                    MustHaveTerms.investigationTerms
                    |> Seq.iter(fun (key) -> 
                        Validate.ParamCollection.ContainsParamWithTerm key investigations
                    )
                }

                //Check if every needed Study Metadata field is there
                ARCExpect.validationCase (TestID.Name "Arc contains all needed Study Metadata fields") {

                    for studySingular in studyMetadata do
                        MustHaveTerms.studyTerms
                        |> Seq.iter(fun (key) -> 
                            Validate.ParamCollection.ContainsParamWithTerm key studySingular
                        )
                }

                //Check if every needed Assay Metadata field is there
                ARCExpect.validationCase (TestID.Name "Arc contains all needed Assay Metadata fields") {

                    for assaySingular in assayMetadata do
                        MustHaveTerms.assayTerms
                        |> Seq.iter(fun (key) -> 
                            Validate.ParamCollection.ContainsParamWithTerm key assaySingular
                        )
                }
                
                //Check if Investigation metadata contains optional fields
                ARCExpect.validationCase (TestID.Name "Arc contains Investigation optional Metadata fields") {
                    
                    let investigations = 
                        investigationMetadata|>List.concat
                    MayHave.investigationTerms
                    |> Seq.iter(fun (key,valSeq) -> 
                        if investigations|>Seq.exists(fun x -> (Param.getCvAccession x = key.Accession)) then
                        
                            valSeq
                            |> Seq.iter(fun x -> Validate.ParamCollection.ContainsParamWithTerm x investigations)
                    )
                }

                //Check if Study metadata contains optional fields
                ARCExpect.validationCase (TestID.Name "Arc contains Study optional Metadata fields") {
                    
                    for studySingular in studyMetadata do
                        MayHave.studyTerms
                        |> Seq.iter(fun (key,valSeq) -> 
                            if studySingular|>Seq.exists(fun x -> (Param.getCvAccession x = key.Accession)) then

                                valSeq
                                |> Seq.iter(fun x -> Validate.ParamCollection.ContainsParamWithTerm x studySingular)
                        )
                }
            ]

        ARCValidationPackage.create(
            metadata = ValidationPackageMetadata.create(
                name = "arc_specification",
                summary = "Validate whether an ARC conforms to Specification V2.0.0-draft",
                description = "Validate whether an ARC conforms to Specification V2.0.0-draft. See the relevant spec at https://github.com/nfdi4plants/ARC-specification/blob/v2.0.0/ARC%20specification.md",
                majorVersion = 2,
                minorVersion = 0,
                patchVersion = 0
            ),
            CriticalValidationCasesList = cases
        )