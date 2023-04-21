#r "nuget: FSharpAux"
#r @"C:\Repos\nfdi4plants\ArcGraphModel\src\ArcGraphModel\bin\Debug\net6.0\ArcGraphModel.dll"
#r @"C:\Repos\CSBiology\FsSpreadsheet\src\FsSpreadsheet\bin\Debug\netstandard2.0\FsSpreadsheet.dll"
//#r @

// TO DO: write validation tests that WORK ON CVPARAMS (list/array) instead of graph!

// look for a function that checks if a Raw/Derived Data file is present in the ARC (look in arc-validate)
// look if Input/Output columns have empty data cells

open ArcGraphModel
open ArcGraphModel.ArcType
open FSharpAux
open FsSpreadsheet

let address = [CvParam("", "Address", "", ParamValue.Value "annotationTable1!A2")]
let address2 = [CvParam("", "Address", "", ParamValue.Value "annotationTable1!B2")]
let address3 = [CvParam("", "Address", "", ParamValue.Value "annotationTable1!D5")]
let testCvP3' = CvParam("ISA:0001", "Sample Name", "ISA", ParamValue.Value "", address3)
address.Head :> IParamBase |> ParamBase.getValue |> string
//address.Head :> IParamBase |> ParamBase.getValue :?> string

let testCvP = CvParam("ISA:0000", "Source Name", "ISA", ParamValue.Value "eppi1", address)
let testCvP2 = CvParam("ISA:0000", "Source Name", "ISA", ParamValue.Value "", address2)
let testCvP3 = CvParam("ISA:0001", "Sample Name", "ISA", ParamValue.Value "", address3)
testCvP["Address"] :> IParamBase |> ParamBase.getValue
testCvP :> IParamBase |> ParamBase.getValue



returnEmptyInputCells [testCvP; testCvP2; testCvP3]
returnEmptyOutputCells [testCvP; testCvP2; testCvP3]

let address4 = [CvParam("", "Address", "", ParamValue.Value "annotationTable1!C2")]
let ddfp = @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\testARC30\assays\aid\dataset\myDerivedDataFile1.txt"
let testCvP4 = CvParam("ISA:0002", "Derived Data File", "ISA", ParamValue.Value ddfp, address4)
let address5 = [CvParam("", "Address", "", ParamValue.Value "annotationTable1!C3")]
let ddfp2 = @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\testARC30\assays\aid\dataset\myDerivedDataFile2.txt"
let testCvP5 = CvParam("ISA:0002", "Derived Data File", "ISA", ParamValue.Value ddfp2, address5)

BuildingBlockType.tryOfString (testCvP5 :> ICvBase).Name

returnNonExistentFileNames [testCvP; testCvP4; testCvP5]