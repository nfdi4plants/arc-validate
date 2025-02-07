#r "nuget: ARCTokenization"
#r "nuget: ControlledVocabulary"


open ARCTokenization
open ControlledVocabulary


let arcDirPath = @"C:\Repos\git.nfdi4plants.org\ArcPrototype\assays\measurement1\isa.assay.xlsx"
let assProcSeqCvps = Assay.parseProcessGraphColumnsFromFile arcDirPath
let cellLysisWorksheetCvps = Map.find "Cell Lysis" assProcSeqCvps