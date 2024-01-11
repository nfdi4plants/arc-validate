#I "src/ArcValidation/bin/Debug/netstandard2.0"
#I "src/ArcValidation/bin/Release/netstandard2.0"
#r "ARCValidation.dll"
//#I "../ARCTokenization/src/ARCTokenization/bin/Debug/netstandard2.0"
//#I "../ARCTokenization/src/ARCTokenization/bin/Release/netstandard2.0"
//#r "ARCTokenization.dll"
//#r "ControlledVocabulary.dll"

#r "nuget: ARCTokenization"
#r "nuget: Expecto"
#r "nuget: FSharpAux, 1.1.0"
#r "nuget: Graphoscope"
#r "nuget: Cytoscape.NET"
#r "nuget: OBO.NET, 0.3.0"
#r "nuget: FsSpreadsheet.ExcelIO, 4.1.0"



open FsSpreadsheet
open FsSpreadsheet.ExcelIO

let wb = FsWorkbook.fromXlsxFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\studies\experiment1_material\isa.study.xlsx"
let sheetProcesses = ARCTokenization.AnnotationTable.parseWorkbook wb