namespace ArcValidation.Validate.Critical

open ArcValidation

open System.IO
open ErrorMessage
open OntologyHelperFunctions
open FSharpAux

// [DEPRECATED]
// temporarily stays here to remember which cases to cover
//module Checks = 

    //let hasInfoFolder           = Directory.Exists arcPaths.InfoPath
    //let hasObjectsFolder        = Directory.Exists arcPaths.ObjectsPath
    //let hasRefsFolder           = Directory.Exists arcPaths.RefsPath
    //let hasRunsFolder           = Directory.Exists arcPaths.RunsPath
    //let hasWorkflowsFolder      = Directory.Exists arcPaths.WorkflowsPath
    //let hasInvestigationFile    = File.Exists arcPaths.InvestigationPath
    //let studiesFolderStructure  = getElementInElementsFolder studiesPath
    //let allStudies              = checkStudiesFolderStructure studiesFolderStructure

module FilesystemEntry =

    /// Validates the presence of a folderpath.
    let folder folderpath =
        if Directory.Exists folderpath then Success
        else Error (Message.create folderpath None None None MessageKind.FilesystemEntryKind)

    /// Validates the presence of a filepath.
    let file filepath =
        if File.Exists filepath then Success
        else Error (Message.create filepath None None None MessageKind.FilesystemEntryKind)


    module StudyFile =

        /// Validates a Study file's registration in the Investigation.
        let registrationInInvestigation (investigationStudiesPathsAndIds : seq<string option * string option>) studyFilepath =
            let studyFilepathLinux = String.replace "/" "\\" studyFilepath
            let cond = 
                investigationStudiesPathsAndIds
                |> Seq.exists (
                    fun (p,id) -> 
                        let pLinux = Option.map (String.replace "/" "\\") p
                        pLinux = Some studyFilepathLinux
                ) 
            if cond then Success
            else Error (Message.create studyFilepath None None None MessageKind.FilesystemEntryKind)

    module AssayFile =

        /// Validates a Study file's registration in the Investigation.
        let registrationInInvestigation (investigationStudiesPathsAndIds : seq<string option * string option>) studyFilepath =
            let studyFilepathLinux = String.replace "/" "\\" studyFilepath
            let cond = 
                investigationStudiesPathsAndIds
                |> Seq.exists (
                    fun (p,id) -> 
                        let pLinux = Option.map (String.replace "/" "\\") p
                        pLinux = Some studyFilepathLinux
                ) 
            if cond then Success
            else Error (Message.create studyFilepath None None None MessageKind.FilesystemEntryKind)