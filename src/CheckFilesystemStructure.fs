module CheckFilesystemStructure

open FilesystemRepresentation
open System.IO

let inputPath = Directory.GetCurrentDirectory()

let arcFolderPath       = Path.Combine(inputPath, ".arc")
let gitFolderPath       = Path.Combine(inputPath, ".git")
let hooksPath           = Path.Combine(gitFolderPath, "hooks")
let objectsPath         = Path.Combine(gitFolderPath, "objects")
let refsPath            = Path.Combine(gitFolderPath, "refs")
let configPath          = Path.Combine(gitFolderPath, "config")
let studiesPath         = Path.Combine(inputPath, "studies")
let assaysPath          = Path.Combine(inputPath, "assays")
let runsPath            = Path.Combine(inputPath, "runs")
let workflowsPath       = Path.Combine(inputPath, "workflows")
let investigationPath   = Path.Combine(inputPath, "isa.investigation.xlsx")

let hasArcFolder            = Directory.Exists arcFolderPath
let hasGitFolder            = Directory.Exists gitFolderPath
let hasHooksFolder          = Directory.Exists hooksPath
let hasObjectsFolder        = Directory.Exists objectsPath
let hasRefsFolder           = Directory.Exists refsPath
let hasStudiesFolder        = Directory.Exists studiesPath
let hasAssaysFolder         = Directory.Exists assaysPath
let hasRunsFolder           = Directory.Exists runsPath
let hasWorkflowsFolder      = Directory.Exists workflowsPath
let hasInvestigationFile    = File.Exists investigationPath
let studiesFolderStructure  = 
    if hasStudiesFolder then getElementInElementsFolder 