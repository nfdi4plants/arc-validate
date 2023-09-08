namespace ArcValidation


/// Representation of the relation between different terms in ARC-related ontologies.
[<System.Flags>]
type ArcRelation = 
    | IsA = 1
    | PartOf = 2
    | HasA = 4
    | Follows = 8
    | Unknown = 16

/// Functions for working with ArcRelations.
module ArcRelation =

    /// Takes a relationship in the form of a string and returns the respective ArcRelation. Every relationship that cannot be represented will be returned as Unknown ArcRelation.
    let toArcRelation relationship =
        match relationship with
        | "part_of" -> ArcRelation.PartOf
        | "is_a" -> ArcRelation.IsA
        | "has_a" -> ArcRelation.HasA
        | "follows" -> ArcRelation.Follows
        //| _ -> failwith $"Relationship {relationship} is no supported ArcRelation."
        | _ -> ArcRelation.Unknown