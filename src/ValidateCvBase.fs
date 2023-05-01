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
        //| _ -> Error (ErrorMessage.XlsxFile.createFromCvParam person)
        // TO DO: Rewrite this with own CvParam creation (instead of using HLW's one) which has all ErrorMessage-related information inside
        | _ -> Error (ErrorMessage.FilesystemEntry.createFromCvParam person)

    /// Validates several persons.
    let persons (persons : CvContainer seq) =
        Seq.map person persons

    /// Validates a filepath.
    let filepath<'T when 'T :> CvParam> (filepath : 'T) =
        match CvParam.tryGetAttribute "Filepath" filepath |> Option.map Param.getValueAsString with
        | Some fp -> Success
        //| None -> Error (ErrorMessage.XlsxFile.createFromCvParam filepath)
        // TO DO: Rewrite this with CvParam creation (instead of using HLW's one) which has all ErrorMessage-related information inside
        | None -> Error (ErrorMessage.FilesystemEntry.createFromCvParam filepath)

    /// Validates several filepaths.
    let filepaths (filepaths : CvParam seq) =
        Seq.map filepath filepaths

    /// Validates if CvContainer contains persons.
    let contacts investigationPath (containers : CvContainer seq) =
        if Seq.length containers > 0 then Success
        // TO DO: incorporate cell information (e.g. INVESTIGATION CONTACTS cell)
        //else Error (ErrorMessage.XlsxFile.createFromCvContainer containers)
        else Error (ErrorMessage.FilesystemEntry.createFromFile investigationPath)