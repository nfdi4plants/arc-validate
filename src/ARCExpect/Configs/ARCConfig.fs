namespace ARCExpect.Configs
open ARCExpect
open System.IO

type ARCConfig(pathConfig : PathConfig) = 

    new(arcRootPath : string) = 
        let pathConfig = new PathConfig(arcRootPath)
        ARCConfig(pathConfig)

    member val PathConfig = pathConfig with get, set

    // Investigation (might be worth to separate into mutiple configs)

    member this.InvestigationWorkbook = ()

    member this.InvestigationWorksheet = ()

    member this.InvestigationPathCvP = ()

    member this.InvestigationTokens = ()

    member this.InvestigationContainers = ()

    member this.InvestigationContainer = ()

    member this.InvestigationStudies = ()

    member this.InvestigationContactsContainer = ()

    // Study (might be worth to separate into mutiple configs)

    member this.StudyPathsAndIds = ()
    
    member this.StudyFolders = ()

    member this.StudyFilesAndIds = ()

    member this.StudyAnnotationTables = ()

    member this.StudyRawOrDerivedDataPaths = ()

    // Assay (might be worth to separate into mutiple configs)

    member this.AssayFolders = ()

    member this.AssayFilesAndIds = ()

    member this.AssayAnnotationTables = ()

    member this.AssayRawOrDerivedDataPaths = ()

    // Composite

    //member this.DataPaths = Seq.append this.StudyRawOrDerivedDataPaths this.AssayRawOrDerivedDataPaths
    member this.DataPaths = ()
