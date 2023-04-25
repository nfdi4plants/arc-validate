/// Functions to validate #ICvBase entities.
module ValidateCvBase

open ArcGraphModel
open FSharpAux
open OntologyHelperFunctions
open FsSpreadsheet
open AuxExt


let person<'T when 'T :> CvParam> (person : 'T) =
    //let firstName : string option = CvContainer.tryGetSingleAs "FirstName" person |> Option.bind f
    let firstName = CvParam.tryGetAttribute "given name" person |> Option.map Param.getValueAsString
    let lastName = CvParam.tryGetAttribute "family name" person |> Option.map Param.getValueAsString
    //let lastName = CvContainer.tryGetSingleAs "LastName" person |> Option.bind  f
    let message = 
        let line = CvParam.tryGetAttribute "Row" person |> Option.get |> Param.getValueAsInt
        let sheet = CvParam.tryGetAttribute "Sheetname" person |> Option.get |> Param.getValueAsString
        let pos = CvParam.tryGetAttribute "Column" person |> Option.get |> Param.getValueAsInt
        let path = CvParam.tryGetAttribute "Filepath" person |> Option.get |> Param.getValueAsString
        createMessage path (Some line) (Some pos) (Some sheet) XLSXFile
    match String.isNoneOrWhiteSpace firstName, String.isNoneOrWhiteSpace lastName with
    | false, false -> Success
    | _ -> Error message

let persons (persons : CvParam list) =
    List.map person persons

//let filepath<'T when 'T :> CvParam> (filepath : 'T) message =
        