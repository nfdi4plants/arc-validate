module ArcGraph


open ControlledVocabulary
open ARCTokenization
open Graphoscope


/// Representation of the different kinds of relations OboTerms can have with each other.
type Relation = 
    /// is_a relation with field link.
    | IsA of string
    /// part_of relation with field link.
    | PartOf of string
    /// has_a relation with field link.
    | HasA of string
    /// Follows relation with field link.
    | Follows of string
    /// Custom relation in the form of field name * link.
    | Custom of string * string

/// Takes a list of CvParams and returns the ArcGraph as an FGraph consisting of Nodes only.
let fromCvParamList cvpList =
    cvpList
    |> List.mapi (
        fun i cvp ->
            (i,CvBase.getCvName cvp), cvp
    )
    |> FGraph.createFromNodes<int*string,CvParam,Relation> 

