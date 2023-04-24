module ValidateCvBase

open ArcGraphModel
open FSharpAux
open OntologyHelperFunctions
open FsSpreadsheet
open AuxExt


/// Functions to validate #ICvBase entities.
module Validate =

    let person<'T when 'T :> CvContainer> (person : 'T) =
        //let firstName : string option = CvContainer.tryGetSingleAs "FirstName" person |> Option.bind f
        let firstName = CvContainer.tryGetSingleParam "FirstName" person |> Option.map Param.getValueAsString
        let lastName = CvContainer.tryGetSingleParam "LastName" person |> Option.map Param.getValueAsString
        //let lastName = CvContainer.tryGetSingleAs "LastName" person |> Option.bind  f
        let message = 
            let sheet, line, pos =
                //CvContainer.tryGetSingleAs "Address" person
                CvContainer.tryGetSingleParam "Address" person
                //|> Option.bind f
                |> Option.get
                |> Param.getValueAsString
                |> String.splitAddress
            //let path = CvContainer.tryGetSingleAs "Filepath" person |> Option.bind f |> Option.get
            let path = CvContainer.tryGetSingleParam "Filepath" person |> Option.map Param.getValueAsString |> Option.get
            createMessage path (Some line) (Some pos) (Some sheet) XLSXFile
        match String.isNoneOrWhiteSpace firstName, String.isNoneOrWhiteSpace lastName with
        | false, false -> Success
        | _ -> Error message
        //0

    let persons (persons : CvContainer list) =
        persons
        |> List.map (person >> (=) Success)

    //let filepath<'T when 'T :> CvParam> (filepath : 'T) message =
        