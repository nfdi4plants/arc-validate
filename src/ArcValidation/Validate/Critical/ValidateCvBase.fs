namespace ArcValidation.Validate.Critical

open ArcValidation

open ArcGraphModel
open FSharpAux
open OntologyHelperFunctions
open CvTokenHelperFunctions
open System.IO


/// Functions to validate #ICvBase entities.
module CvBase =

    /// Validates a CvContainer's given property.
    let property<'T when 'T :> CvContainer> property (cvContainer : 'T) =
        let containerProperty = CvContainer.tryGetPropertyStringValue property cvContainer
        if String.isNoneOrWhiteSpace containerProperty then
            // TO DO: Rewrite this with own CvParam creation (instead of using HLW's one) which has all ErrorMessage-related information inside
            // Error (ErrorMessage.XlsxFile.createFromCvParam personCvContainer)
            Error (ErrorMessage.FilesystemEntry.createFromCvParam cvContainer)
        else Success

    module Person =

        /// Validates a person's email address.
        let emailAddress<'T when 'T :> CvContainer> (personCvContainer : 'T) =
            property "E-mail Address" personCvContainer

        /// Validates a person's first name.
        let firstName<'T when 'T :> CvContainer> (personCvContainer : 'T) =
            property "given name" personCvContainer

        /// Validates a person's last name.
        let lastName<'T when 'T :> CvContainer> (personCvContainer : 'T) =
            property "family name" personCvContainer

        /// Validates a person's affiliation.
        let affiliation<'T when 'T :> CvContainer> (personCvContainer : 'T) =
            property "Affiliation" personCvContainer

        /// Validates a person's ORCID.
        let orcid<'T when 'T :> CvContainer> (personCvContainer : 'T) = 
            let orcidProperty = CvContainer.tryGetPropertyStringValue "<  ORCID>" personCvContainer
            let rgxPat = System.Text.RegularExpressions.Regex("^\d{4}-\d{4}-\d{4}-\d{4}$")
            let err = Error (ErrorMessage.FilesystemEntry.createFromCvParam personCvContainer)
            match orcidProperty with
            | s when String.isNoneOrWhiteSpace s -> err
            | o when rgxPat.Match(o.Value).Success |> not -> err
            | _ -> Success

    /// Validates several persons' given properties.
    let persons properties (personCvContainers : CvContainer seq) =
        properties
        |> Seq.collect (
            fun person ->
                personCvContainers
                |> Seq.map (property person)
        )

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

    /// Validates if a CvContainer collection contains a description.
    let description investigationPath investigationContainers =
        let error = Error (ErrorMessage.FilesystemEntry.createFromFile investigationPath)
        match Seq.tryHead investigationContainers with
        | Some c    -> property "descriptor" c
        | None      -> error

    /// Validates if a CvContainer collection contains a title.
    let title investigationPath investigationContainers =
        let error = Error (ErrorMessage.FilesystemEntry.createFromFile investigationPath)
        match Seq.tryHead investigationContainers with
        | Some c    -> property "title" c
        | None      -> error