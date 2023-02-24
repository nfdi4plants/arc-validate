module FilesystemRepresentation

open System.IO

/// Type representation of a Study folder.
type StudyFolderStructure = {
    Name                : string
    Path                : string
    HasIsaFile          : bool
    HasResourcesFolder  : bool
}

/// Creates a StudyFolderStructure from given parameters.
let createStudyFolderStructure name path hasIsaFile hasResourcesFolder = {
    Name                = name
    Path                = path
    HasIsaFile          = hasIsaFile
    HasResourcesFolder  = hasResourcesFolder
}

/// Type representation of an Assay folder.
type AssayFolderStructure = {
    Name                : string
    Path                : string
    HasIsaFile          : bool
    HasDatasetFolder    : bool
}

/// Creates an AssayFolderStructure from given parameters.
let createAssayFolderStructure name path hasIsaFile hasDatasetFolder = {
    Name                = name
    Path                = path
    HasIsaFile          = hasIsaFile
    HasDatasetFolder    = hasDatasetFolder
}

/// Type representation of a Workflow folder.
type WorkflowFolderStructure = {
    Name                : string
    Path                : string
    HasWorkflowFile     : bool
}

/// Creates a WorkflowFolderStructure from given parameters.
let createWorkflowFolderStructure name path hasWorkflowFile = {
    Name                = name
    Path                = path
    HasWorkflowFile     = hasWorkflowFile
}

/// Type representation of a Run folder.
type RunFolderStructure = {
    Name                : string
    Path                : string
    HasRunFile          : bool
    HasOutputFiles      : bool
}

/// Creates a WorkflowFolderStructure from given parameters.
let createRunFolderStructure name path hasRunFile hasOutputFiles = {
    Name                = name
    Path                = path
    HasRunFile          = hasRunFile
    HasOutputFiles      = hasOutputFiles
}

/// Takes a possible Elements folder path (`string option`) and returns the possible collection of all Elements folders' paths in it. This applies to Studies, Assays, Workflows, and Runs as Elements.
let getElementInElementsFolder elementsFolder =
    if Directory.Exists elementsFolder then Directory.GetDirectories elementsFolder |> Some
    else None

/// Takes a function specified to transform an input `string` into an Element folder structure and a possible collection of Elements' paths (`string [] option`) and returns the possible folder structure for each Element.
let private checkElementsFolderStructure elemFunction (elementsInElementsfolder : string [] option) =
    match elementsInElementsfolder with
    | None -> None
    | Some elements ->
        elements
        |> Array.map elemFunction
        |> Some

/// Takes a possible collection of Studies' paths (`string [] option`) and returns the possible folder structure of each Study.
let checkStudiesFolderStructure studiesInStudiesFolder =
    let elemFunction dir =
        let n = (DirectoryInfo dir).Name
        let isaf = Path.Combine(dir, "isa.study.xlsx") |> File.Exists
        let rf = Path.Combine(dir, "resources") |> Directory.Exists
        createStudyFolderStructure n dir isaf rf
    checkElementsFolderStructure elemFunction studiesInStudiesFolder

/// Takes a possible collection of Assays' paths (`string [] option`) and returns the possible folder structure of each Assay.
let checkAssaysFolderStructure assaysInAssaysFolder =
    let elemFunction dir =
        let n = (DirectoryInfo dir).Name
        let isaf = Path.Combine(dir, "isa.assay.xlsx") |> File.Exists
        let df = Path.Combine(dir, "dataset") |> Directory.Exists
        createAssayFolderStructure n dir isaf df
    checkElementsFolderStructure elemFunction assaysInAssaysFolder

/// Takes a possible collection of Workflows' paths (`string [] option`) and returns the possible folder structure of each Workflow.
let checkWorkflowsFolderStructure workflowsInWorkflowsFolder =
    let elemFunction dir =
        let n = (DirectoryInfo dir).Name
        let wff = Path.Combine(dir, "workflow.cwl") |> File.Exists
        createWorkflowFolderStructure n dir wff
    checkElementsFolderStructure elemFunction workflowsInWorkflowsFolder

/// Takes a possible collection of Runs' paths (`string [] option`) and returns the possible folder structure of each Run.
let checkRunsFolderStructure runsInRunsFolder =
    let elemFunction dir =
        let n = (DirectoryInfo dir).Name
        let rf = Path.Combine(dir, "run.cwl") |> File.Exists
        let opf = 
            let af = Directory.GetFiles dir
            match af.Length with
            | 0 -> false
            | _ -> af |> Array.exists (fun f -> (FileInfo f).Name <> "run.cwl" )
        createRunFolderStructure n dir rf opf
    checkElementsFolderStructure elemFunction runsInRunsFolder