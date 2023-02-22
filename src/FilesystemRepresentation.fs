module FilesystemRepresentation

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