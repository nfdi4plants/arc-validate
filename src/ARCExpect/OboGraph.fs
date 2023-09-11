namespace ARCExpect


open ARCExpect.ARCRelation

open FsOboParser
open Graphoscope


/// Functions for working with ontology-based FGraphs.
module OboGraph =

    /// Takes a TermRelation of string and returns the approbriate ARCRelation plus source term and target term as a triple. If the TermRelation is Empty or TargetMissing, returns None.
    let tryToARCRelation termRelation =
        match termRelation with
        | Empty t -> None
        | TargetMissing (r,t) -> None
        | Target (r,st,tt) -> Some (toARCRelation r,st,tt)

    /// Takes an OboOntology and returns an FGraph with OboTerms as nodes and ARCRelations as Edges. The structure of the graph results from the TermRelations between the ontology's terms.
    let ontologyToFGraph onto =
        OboOntology.getRelations onto
        |> Seq.choose tryToARCRelation
        |> Seq.groupBy (
            fun (r,st,tt) -> 
                st, tt
        )
        |> Seq.map (
            fun (k,v) -> 
                v 
                |> Seq.reduce (
                    fun (ar1,st1,tt1) (ar2,st2,tt2) -> 
                        ar1 + ar2, st1, tt1
                )
        )
        |> Seq.fold (
            fun acc (ar,st,tt) -> 
                FGraph.addElement st.Id st tt.Id tt ar acc
        ) FGraph.empty<string,OboTerm,ARCRelation>
