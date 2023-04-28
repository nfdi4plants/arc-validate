namespace Validate

open ArcGraphModel
open FSharpAux
open OntologyHelperFunctions
open CvTokenHelperFunctions


/// Functions to validate #ICvBase entities.
module CvBase =

    /// Validates a person.
    let person<'T when 'T :> CvContainer> (person : 'T) =
        let firstName = CvContainer.tryGetPropertyStringValue "given name" person
        let lastName = CvContainer.tryGetPropertyStringValue "family name" person
        match String.isNoneOrWhiteSpace firstName, String.isNoneOrWhiteSpace lastName with
        | false, false -> Success
        | _ -> Error (ErrorMessage.XlsxFile.createFromCvParam person)

    /// Validates several persons.
    let persons (persons : CvContainer seq) =
        Seq.map person persons

    /// Validates a filepath.
    let filepath<'T when 'T :> CvParam> (filepath : 'T) =
        match CvParam.tryGetAttribute "Filepath" filepath |> Option.map Param.getValueAsString with
        | Some fp -> Success
        | None -> Error (ErrorMessage.XlsxFile.createFromCvParam filepath)

    /// Validates several filepaths.
    let filepaths (filepaths : CvParam seq) =
        Seq.map filepath filepaths

    /// Validates if CvContainer contains persons.
    let contacts (container : CvContainer) =
        if container |> CvBase.equalsTerm Terms.person then Success
        else Error (ErrorMessage.XlsxFile.createFromCvContainer container)