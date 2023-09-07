namespace ArcValidation.Validate.NonCritical

open ArcValidation

open ArcGraphModel
open FSharpAux
open OntologyHelperFunctions
open CvTokenHelperFunctions
open System.IO


/// Functions to validate #ICvBase entities.
module CvBase =

    module Person =

        /// Validates a person's ORCID.
        let orcid<'T when 'T :> CvContainer> (personCvContainer : 'T) = 
            let orcidProperty = CvContainer.tryGetPropertyStringValue "<  ORCID>" personCvContainer
            printfn "ORCID property: %A" orcidProperty
            let err = Error (ErrorMessage.FilesystemEntry.createFromCvParam personCvContainer)
            match orcidProperty with
            //| s when String.isNoneOrWhiteSpace s -> err
            | o when InternalUtils.Orcid.checkValid o.Value |> not -> err
            | _ -> Success