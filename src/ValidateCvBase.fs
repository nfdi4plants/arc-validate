module ValidateCvBase

open ArcGraphModel
open ArcGraphModel.ArcType
open FSharpAux

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

let containsFilePath cvParam =
    returnCellAddressWhen (Ont)

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