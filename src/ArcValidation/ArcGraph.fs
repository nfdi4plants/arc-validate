namespace ArcValidation


open ControlledVocabulary
open ARCTokenization
open Graphoscope
open FsOboParser


module ArcGraph =

    /// Takes a list of CvParams and returns the ArcGraph as an FGraph consisting of Nodes only.
    let fromCvParamList cvpList =
        cvpList
        |> List.mapi (
            fun i cvp ->
                (i,CvBase.getCvName cvp), cvp
        )
        |> FGraph.createFromNodes<int*string,CvParam,ArcRelation> 

    /// Returns all related Terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph.
    let getRelatedCvParams (cvp : CvParam) (graph : FGraph<string,OboTerm,ArcRelation>) =
        FContext.successors graph[cvp.Accession]
        |> Seq.map (fun (id,rel) -> FGraph.findNode id graph, rel)
        |> Seq.map (fun ((id,t),r) -> id, t, r)

    /// Checks is a given current CvParam has a given ArcRelation to a given prior CvParam by using a given ontology graph.
    let hasRelationTo onto (relation : ArcRelation) currentCvp (priorCvp : CvParam) =
        getRelatedCvParams currentCvp onto
        |> Seq.exists (fun (id,t,r) -> id = priorCvp.Accession && r.HasFlag relation)

    /// Checks is a given current CvParam has a follows relationship to a given prior CvParam by using a given ontology graph.
    let hasFollowsTo onto currentCvp priorCvp =
        hasRelationTo onto ArcRelation.Follows currentCvp priorCvp

    /// Checks is a given current CvParam has a part_of relationship to a given prior CvParam by using a given OboOntology.
    let hasPartOfTo onto currentCvp priorCvp =
        hasRelationTo onto ArcRelation.PartOf currentCvp priorCvp

    /// Checks if 2 given CvParams share the same ArcRelation to the same other term.
    let equalsRelation onto (relation : ArcRelation) cvp1 cvp2 =
        let relTermsCvp1 = getRelatedCvParams cvp1 onto |> Seq.filter (fun (id,t,r) -> r.HasFlag relation)
        let relTermsCvp2 = getRelatedCvParams cvp2 onto |> Seq.filter (fun (id,t,r) -> r.HasFlag relation)
        relTermsCvp1
        |> Seq.exists (
            fun (id1,t1,r1) -> 
                relTermsCvp2
                |> Seq.exists (
                    fun (id2,t2,r2) -> 
                        t1 = t2 && r2.HasFlag relation && r1.HasFlag relation
                )
        )

    /// Takes an ontology-based FGraph and returns a seq of OboTerms that are endpoints. Endpoints are OboTerms that have no relation pointing at them.
    let getEndpoints (onto : FGraph<string,OboTerm,ArcRelation>) =
        onto.Values
        |> Seq.map (fun c -> c |> fun (id,t,e) -> t, FContext.predecessors c)
        |> Seq.map (fun (t,p) -> t, p |> Seq.filter (fun (id,r) -> r.HasFlag ArcRelation.PartOf))
        |> Seq.choose (fun (t,p) -> if Seq.length p = 0 then Some t else None)

    /// Takes an OboTerm seq of endpoints (that is, any term without predecessors) and filters a list of CvParams where every CvParam that is an endpoint is excluded.
    let deleteEndpointSectionKeys (ontoEndpoints : OboTerm seq) (cvParams : CvParam list) =
        cvParams
        |> List.filter (
            fun cvp ->
                ontoEndpoints
                |> Seq.exists (
                    fun t ->
                        t.Name = cvp.Name &&
                        t.Id = cvp.Accession &&
                        CvParam.getValueAsTerm cvp |> fun cvt -> cvt.Accession = "AGMO:00000001" && cvt.Name = "Metadata Section Key"
                )
                |> not
        )