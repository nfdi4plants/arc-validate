#r "nuget: FSharpAux"
#r @"C:\Repos\nfdi4plants\ArcGraphModel\src\ArcGraphModel\bin\Debug\net6.0\ArcGraphModel.dll"
//#r @""

// TO DO: write validation tests that WORK ON CVPARAMS (list/array) instead of graph!

// look for a function that checks if a Raw/Derived Data file is present in the ARC (look in arc-validate)
// look if Input/Output columns have empty data cells

open ArcGraphModel
open ArcGraphModel.ArcType
open FSharpAux

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

/// Returns a CvParam's cell address (in the form of `<sheetname>!<column letter><row number>`, e.g. `sheet1!A1`) for a given column
/// type if `predicate` returns true. Else returns None.
let returnCellAddressWhen predicate columnType (cvParam : CvParam) =
    match BuildingBlockType.tryOfString (cvParam :> ICvBase).Name with
    | Some x when columnType x ->
        let isEmpty = 
            cvParam
            :> IParamBase 
            |> ParamBase.getValue 
            |> string
            |> predicate
        if isEmpty then Some (cvParam["Address"] :> IParamBase |> ParamBase.getValue |> string)
        else None
    | _ -> None

/// Returns all Source Name cells with an empty value.
let returnEmptyInputCells cvParams = 
    cvParams |> List.choose (returnCellAddressWhen String.isNullOrEmpty (fun c -> c.IsInputColumn))

/// Returns all Sample Name / Raw Data File / Derived Data File cells with an empty value.
let returnEmptyOutputCells cvParams = 
    cvParams |> List.choose (returnCellAddressWhen String.isNullOrEmpty (fun c -> c.IsOutputColumn))

/// Returns all RawDataFile names, DerivedDataFile names and ProtocolREF filenames that do not exist in the ARC.
let returnNonExistentFileNames cvParams =
    // root path to ARC folder must be part of the string at this point already
    cvParams |> List.choose (returnCellAddressWhen (System.IO.File.Exists >> not) (fun c -> c = RawDataFile || c = DerivedDataFile || c = ProtocolREF))

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