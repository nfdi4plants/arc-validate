namespace ArcValidation.Configs
open ArcValidation
open System.IO

type ArcConfig(pathConfig:PathConfig) = 

    new(arcRootPath: string) = 
        let pathConfig = new PathConfig(arcRootPath)
        ArcConfig(pathConfig)

    member val PathConfig = pathConfig with get, set

    // Investigation (might be worth to separate into mutiple configs)

    member this.InvestigationWorkbook = InformationExtraction.Investigation.getWorkbook this.PathConfig.InvestigationPath

    member this.InvestigationWorksheet = InformationExtraction.Investigation.getWorksheet this.InvestigationWorkbook

    member this.InvestigationPathCvP = InformationExtraction.Investigation.getPathCvP this.PathConfig.InvestigationPath

    member this.InvestigationTokens = InformationExtraction.Investigation.getTokens this.InvestigationPathCvP this.InvestigationWorksheet

    member this.InvestigationContainers = InformationExtraction.Investigation.getContainers this.InvestigationTokens

    member this.InvestigationStudies = InformationExtraction.Investigation.getStudies this.InvestigationContainers

    member this.InvestigationContactsContainer = InformationExtraction.Investigation.getContactsContainer this.InvestigationContainers

    // Study (might be worth to separate into mutiple configs)

    member this.StudyPathsAndIds = InformationExtraction.Study.getPathsAndIds this.PathConfig.StudiesPath this.InvestigationStudies
    
    member this.StudyFolders = InformationExtraction.Study.getFolders this.PathConfig.StudiesPath

    member this.StudyFilesAndIds = InformationExtraction.Study.getFilesAndIds this.StudyFolders

    member this.StudyAnnotationTables = InformationExtraction.Study.getAnnotationTables this.StudyFilesAndIds

    member this.StudyRawOrDerivedDataPaths = InformationExtraction.Study.getRawOrDerivedDataPaths this.StudyAnnotationTables

    // Assay (might be worth to separate into mutiple configs)

    member this.AssayFolders = InformationExtraction.Assay.getFolders this.PathConfig.AssaysPath

    member this.AssayFilesAndIds = InformationExtraction.Assay.getFilesAndIds this.AssayFolders

    member this.AssayAnnotationTables = InformationExtraction.Assay.getAnnotationTables this.AssayFilesAndIds

    member this.AssayRawOrDerivedDataPaths = InformationExtraction.Assay.getRawOrDerivedDataPaths this.AssayAnnotationTables

    // Composite

    member this.DataPaths = Seq.append this.StudyRawOrDerivedDataPaths this.AssayRawOrDerivedDataPaths
