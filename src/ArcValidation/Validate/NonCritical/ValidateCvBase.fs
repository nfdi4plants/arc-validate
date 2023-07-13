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

        open ArcValidation.Validate.Critical.CvBase

        /// Validates a person's ORCID.
        let orcid<'T when 'T :> CvContainer> (personCvContainer : 'T) = 
            property "<  ORCID>" personCvContainer