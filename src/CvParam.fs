module CvParam

open ArcGraphModel
open ArcGraphModel.ArcType


/// Returns a CvParam's cell address (in the form of `<sheetname>!<column letter><row number>`, e.g. `sheet1!A1`) for a given column
/// type if `predicate` returns true. Else returns None.
let returnCellAddressWhen predicate columnType (cvParam : CvParam) =
    match BuildingBlockType.tryOfString (cvParam :> ICvBase).Name with
    | Some x when columnType x ->
        let isPredicate = 
            CvParam.getValueAsString cvParam
            |> predicate
        if isPredicate then CvParam.tryGetQualifierValueAsString "Path" cvParam
        else None
    | _ -> None

/// Returns true if a given CvParam contains a Filepath as its ParamValue.
let containsFilePath cvParam =
    let buildingBlockCondition = fun c -> c = RawDataFile || c = DerivedDataFile || c = ProtocolREF
    returnCellAddressWhen String.isFilepath buildingBlockCondition cvParam

//let returnIncorrectFilePathCells cvParams =
//    cvParams |> List.choose (returnCellAddressWhen ())

/// Returns all Source Name cells with an empty value.
let returnEmptyInputCells cvParams = 
    cvParams |> List.choose (returnCellAddressWhen String.isNullOrWhiteSpace (fun c -> c.IsInputColumn))

/// Returns all Sample Name / Raw Data File / Derived Data File cells with an empty value.
let returnEmptyOutputCells cvParams = 
    cvParams |> List.choose (returnCellAddressWhen String.isNullOrWhiteSpace (fun c -> c.IsOutputColumn))

/// Returns all RawDataFile names, DerivedDataFile names and ProtocolREF filenames that do not exist in the ARC.
let returnNonexistentFileNames cvParams =
    // root path to ARC folder must be part of the string at this point already
    cvParams |> List.choose (returnCellAddressWhen (System.IO.File.Exists >> not) (fun c -> c = RawDataFile || c = DerivedDataFile || c = ProtocolREF))