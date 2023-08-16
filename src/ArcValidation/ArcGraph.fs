namespace ArcValidation


open ControlledVocabulary
open ARCTokenization
open Graphoscope


module ArcGraph =

    /// Representation of the different kinds of relations OboTerms can have with each other.
    type Relation = 
        /// is_a relation.
        | IsA = 1
        /// part_of relation.
        | PartOf = 2
        /// has_a relation.
        | HasA = 4
        /// Follows relation.
        | Follows = 8
        ///// Custom relation in the form of field name * link.
        //| Custom of string * string

    /// Takes a list of CvParams and returns the ArcGraph as an FGraph consisting of Nodes only.
    let fromCvParamList cvpList =
        cvpList
        |> List.mapi (
            fun i cvp ->
                (i,CvBase.getCvName cvp), cvp
        )
        |> FGraph.createFromNodes<int*string,CvParam,Relation> 

