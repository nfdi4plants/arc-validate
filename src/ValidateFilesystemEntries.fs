namespace Validate

open System.IO
open ErrorMessage
open OntologyHelperFunctions
open FSharpAux


// [DEPRECATED]
// temporarily stays here to remember which cases to cover
module Checks =

    open ArcPaths

    let hasInfoFolder           = Directory.Exists infoPath
    let hasObjectsFolder        = Directory.Exists objectsPath
    let hasRefsFolder           = Directory.Exists refsPath
    let hasRunsFolder           = Directory.Exists runsPath
    let hasWorkflowsFolder      = Directory.Exists workflowsPath
    let hasInvestigationFile    = File.Exists investigationPath
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