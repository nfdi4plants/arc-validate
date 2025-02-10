#r "nuget: ARCTokenization"
#r "nuget: ControlledVocabulary"


open ARCTokenization
open ControlledVocabulary


let arcDirPath = @"C:\Repos\git.nfdi4plants.org\ArcPrototype\assays\measurement1\isa.assay.xlsx"
let assProcSeqCvps = Assay.parseProcessGraphColumnsFromFile arcDirPath
let cellLysisWorksheetCvps = Map.find "Cell Lysis" assProcSeqCvps

cellLysisWorksheetCvps.Head
// okay, structure (in 1D list = table row) is: 1. item: Header; all subsequent items: values below header
cellLysisWorksheetCvps[1]
// structured as: CvParam with ID of structural term (e.g. Characteristics) and Value of either string (when custom) or CvValue (when term)