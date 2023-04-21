module ValidateCvBase

open ArcGraphModel
open FSharpAux
open OntologyHelperFunctions
open FsSpreadsheet
open AuxExt


/// Functions to validate #ICvBase entities.
module Validate =

    //let f (v : ICvBase) =
    //    match v with
    //    | :? CvParam as cvp -> cvp |> ParamBase.getValue |> string |> Some
    //    | _ -> None

    let person<'T when 'T :> CvContainer> (person : 'T) =
        //let firstName : string option = CvContainer.tryGetSingleAs "FirstName" person |> Option.bind f
        let firstName = CvContainer.tryGetSingleAsValue "FirstName" person |> Option.map string
        let lastName = CvContainer.tryGetSingleAsValue "LastName" person |> Option.map string
        //let lastName = CvContainer.tryGetSingleAs "LastName" person |> Option.bind  f
        let message = 
            let sheet, line, pos =
                //CvContainer.tryGetSingleAs "Address" person
                CvContainer.tryGetSingleAsValue "Address" person
                //|> Option.bind f
                |> Option.get
                |> string
                |> String.splitAddress
            //let path = CvContainer.tryGetSingleAs "Filepath" person |> Option.bind f |> Option.get
            let path = CvContainer.tryGetSingleAsValue "Filepath" person |> Option.map string |> Option.get
            createMessage path (Some line) (Some pos) (Some sheet) XLSXFile
        match String.isNoneOrWhiteSpace firstName, String.isNoneOrWhiteSpace lastName with
        | false, false -> Success
        | _ -> Error message
        //0

    let persons (persons : CvContainer list) =
        persons
        |> List.exists (person >> (=) Success)

    //let filepath<'T when 'T :> CvParam> (filepath : 'T) message =
        