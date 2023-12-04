namespace ARCExpect


/// Representation of the relation between different terms in ARC-related ontologies.
[<System.Flags>]
type ARCRelation = 
    | IsA = 1
    | PartOf = 2
    | HasA = 4
    | Follows = 8
    | Unknown = 16
    | Misplaced = 32
    | Obsolete = 64

/// Functions for working with ARCRelations.
module ARCRelation =

    /// Takes a relationship in the form of a string and returns the respective ARCRelation. Every relationship that cannot be represented will be returned as Unknown ARCRelation.
    let toARCRelation relationship =
        match relationship with
        | "part_of" -> ARCRelation.PartOf
        | "is_a" -> ARCRelation.IsA
        | "has_a" -> ARCRelation.HasA
        | "follows" -> ARCRelation.Follows
        //| _ -> failwith $"Relationship {relationship} is no supported ARCRelation."
        | _ -> ARCRelation.Unknown