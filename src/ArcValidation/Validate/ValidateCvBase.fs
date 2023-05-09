namespace ArcValidation.Validate

open ArcValidation

open ArcGraphModel
open FSharpAux
open OntologyHelperFunctions
open CvTokenHelperFunctions
open System.IO


/// Functions to validate #ICvBase entities.
module CvBase =

    /// Validates a person.
    let person<'T when 'T :> CvContainer> (personCvContainer : 'T) =
        let firstName = CvContainer.tryGetPropertyStringValue "given name" personCvContainer
        let lastName = CvContainer.tryGetPropertyStringValue "family name" personCvContainer
        match String.isNoneOrWhiteSpace firstName, String.isNoneOrWhiteSpace lastName with
        | false, false -> Success
        //| _ -> Error (ErrorMessage.XlsxFile.createFromCvParam personCvContainer)
        // TO DO: Rewrite this with own CvParam creation (instead of using HLW's one) which has all ErrorMessage-related information inside
        | _ -> Error (ErrorMessage.FilesystemEntry.createFromCvParam personCvContainer)

    /// Validates several persons.
    let persons (personCvContainers : CvContainer seq) =
        Seq.map person personCvContainers

    /// Validates a filepath.
    let filepath<'T when 'T :> CvParam> (filepathCvParam : 'T) =
        match CvParam.tryGetAttribute "Filepath" filepathCvParam |> Option.map Param.getValueAsString with
        | Some fp -> Success
        //| None -> Error (ErrorMessage.XlsxFile.createFromCvParam filepathCvParam)
        // TO DO: Rewrite this with CvParam creation (instead of using HLW's one) which has all ErrorMessage-related information inside
        | None -> Error (ErrorMessage.FilesystemEntry.createFromCvParam filepathCvParam)

    /// Validates several filepaths.
    let filepaths (filepathCvParams : CvParam seq) =
        Seq.map filepath filepathCvParams

    /// Validates if CvContainer contains persons.
    let contacts investigationPath (contactsContainers : CvContainer seq) =
        if Seq.length contactsContainers > 0 then Success
        // TO DO: incorporate cell information (e.g. INVESTIGATION CONTACTS cell)
        //else Error (ErrorMessage.XlsxFile.createFromCvContainer contactsContainers)
        else Error (ErrorMessage.FilesystemEntry.createFromFile investigationPath)