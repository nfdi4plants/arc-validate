namespace ARCExpect


open FsOboParser
open ControlledVocabulary
open ARCTokenization
open Graphoscope
open Cyjs.NET

open InternalUtils


/// Functions for creating and working with ARC FGraphs.
module ARCGraph =

    /// Takes a list of CvParams and returns the ARCGraph as an FGraph consisting of Nodes only.
    let fromCvParamList cvpList =
        cvpList
        |> List.mapi (
            fun i cvp ->
                (i,CvBase.getCvName cvp), cvp
        )
        |> FGraph.createFromNodes<int*string,CvParam,ARCRelation> 

    /// Returns all related Terms (as ID * OboTerm * ARCRelation) of a given CvParam by using a given ontology graph.
    let getRelatedCvParams (cvp : CvParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
        FContext.successors graph[cvp.Accession]
        |> Seq.map (fun (id,rel) -> FGraph.findNode id graph, rel)
        |> Seq.map (fun ((id,t),r) -> id, t, r)

    /// Checks is a given current CvParam has a given ARCRelation to a given prior CvParam by using a given ontology graph.
    let hasRelationTo onto (relation : ARCRelation) currentCvp (priorCvp : CvParam) =
        getRelatedCvParams currentCvp onto
        |> Seq.exists (fun (id,t,r) -> id = priorCvp.Accession && r.HasFlag relation)

    /// Checks is a given current CvParam has a follows relationship to a given prior CvParam by using a given ontology graph.
    let hasFollowsTo onto currentCvp priorCvp =
        hasRelationTo onto ARCRelation.Follows currentCvp priorCvp

    /// Checks is a given current CvParam has a part_of relationship to a given prior CvParam by using a given OboOntology.
    let hasPartOfTo onto currentCvp priorCvp =
        hasRelationTo onto ARCRelation.PartOf currentCvp priorCvp

    /// Checks if 2 given CvParams share the same ARCRelation to the same other term.
    let equalsRelation onto (relation : ARCRelation) cvp1 cvp2 =
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
    let getEndpoints (onto : FGraph<string,OboTerm,ARCRelation>) =
        onto.Values
        |> Seq.map (fun c -> c |> fun (id,t,e) -> t, FContext.predecessors c)
        |> Seq.map (fun (t,p) -> t, p |> Seq.filter (fun (id,r) -> r.HasFlag ARCRelation.PartOf))
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

    /// Checks if a given CvParam is the header of a given OboTerm sequence.
    let isHeader (ontoEndpoints : OboTerm seq) (cvp : CvParam) =
        ontoEndpoints
        |> Seq.exists (fun t -> t.Name = cvp.Name && t.Id = cvp.Accession)
        |> not

    /// Takes an ontology FGraph and a given CvParam and creates a CvParam based on a CvTerm that has the "Follows" relation to the given CvParam's term. The created CvParam's value is an empty string.
    let createEmptyPriorFollowsCvParam onto cvp =
        getRelatedCvParams cvp onto
        |> Seq.pick (
            fun (id,t,r) -> 
                if r.HasFlag ARCRelation.Follows then 
                    let rowParam = CvParam(Address.row, ParamValue.Value (Param.getValueAsInt cvp["Row"] - 1))
                    let colParam = CvParam(Address.column, ParamValue.Value (Param.getValueAsInt cvp["Column"]))
                    let wsParam = CvParam(Address.worksheet, ParamValue.Value (Param.getValueAsString cvp["Worksheet"]))
                    Some (CvParam(OboTerm.toCvTerm t, ParamValue.Value "", [rowParam; colParam; wsParam]))
                else None
        )

    /// Takes an ontology FGraph and a given CvParam and creates a CvParam based on the CvTerm that the given CvParam's term is related to via the "Follows" relation. The created CvParam's value is an empty string.
    let createEmptySubsequentFollowsCvParamPrior onto cvp =
        ()

    /// Takes an ISA-based ontology in the form of an FGraph and a list of CvParams and creates an FGraph based on a section header's "follows" and "part_of" relations.
    let constructSubgraph isaOntology (cvParams : CvParam list) =
        let nextToSectionHeader currentCvp priorCvp =
            hasPartOfTo isaOntology currentCvp priorCvp
        let follows currentCvp priorCvp =
            hasFollowsTo isaOntology currentCvp priorCvp
        let isaGraph = FGraph.empty<int*string,CvParam,ARCRelation>
        let rec loop (tokens : CvParam list) (stash : CvParam list) (prior : CvParam) parent =
            match tokens with
            | h :: t ->
                match t with
                | [] ->
                    match stash with
                    | [] ->
                        printfn "done via empty stash!"
                    | _ ->
                        printfn $"case new section header: h: {h.Name}, prior: {prior.Name}"
                        loop (h :: stash |> List.rev |> List.tail) t (stash |> List.rev |> List.head) parent
                | _ ->
                    match follows h prior with
                    | true ->
                        printfn "tokensList is %A" (tokens |> List.map (fun (cvp : CvParam) -> $"{cvp.Name}: {cvp.Value |> ParamValue.getValueAsString}"))
                        match nextToSectionHeader h prior with
                        | true ->
                            printfn $"case first term after section header: h: {h.Name}, prior: {prior.Name}"
                            FGraph.addElement (hash h,h.Name) h (hash prior,prior.Name) prior (ARCRelation.PartOf + ARCRelation.Follows) isaGraph |> ignore
                            loop t (prior :: stash) h h
                        | false ->
                            printfn $"case new term: h: {h.Name}, prior: {prior.Name}"
                            FGraph.addElement (hash h,h.Name) h (hash parent,parent.Name) parent ARCRelation.Follows isaGraph |> ignore
                            loop t stash h h
                    | false ->
                        match CvParam.equalsTerm (CvParam.getTerm h) prior with
                        | true ->
                            printfn $"case same term: h: {h.Name}, prior: {prior.Name}"
                            loop t (h :: stash) h parent
                        | false ->
                            printfn $"case term missing: h: {h.Name}, prior: {prior.Name}"
                            let missingTerm = createEmptyPriorFollowsCvParam isaOntology h
                            loop (missingTerm :: h :: t) stash prior parent
            | [] -> 
                printfn "done via empty tokensList! (should not happen...)"
        loop cvParams.Tail [] cvParams.Head cvParams.Head
        isaGraph


    /// Functions for visualizing ARC FGraphs.
    module Visualization =

        /// Takes an FGraph and prints all its nodes and edges by using a given node-transforming function.
        let printGraph transformFunction (graph : FGraph<_,_,_>) =
            for (nk1,nd1,nk2,nd2,e) in FGraph.toSeq graph do
                let nk1s = sprintf "%s" (transformFunction nd1)
                let nk2s = sprintf "%s" (transformFunction nd2)
                printfn "%s ---%A---> %s" nk1s e nk2s

        /// Takes an FGraph and returns a CyGraph by using given functions for transforming node keys, node data and edges.
        let toFullCyGraph nodeKeyTransformer nodeDataTransformer edgeTransformer (fGraph : FGraph<_,_,_>) =
            CyGraph.initEmpty ()
            |> CyGraph.withElements [
                    for (nk1,nd1,nk2,nd2,e) in FGraph.toSeq fGraph do
                        let nk1s = nodeKeyTransformer nk1
                        let nk2s = nodeKeyTransformer nk2
                        Elements.node nk1s [CyParam.label <| nodeDataTransformer nd1]
                        Elements.node nk2s [CyParam.label <| nodeDataTransformer nd2]
                        Elements.edge (sprintf "%s_%s" nk1s nk2s) nk1s nk2s (edgeTransformer e)
                ]
            |> CyGraph.withStyle "node"     
                [
                    CyParam.content =. CyParam.label
                    CyParam.color "#A00975"
                ]
            |> CyGraph.withStyle "edge"     
                [
                    //CyParam.content =. CyParam.label
                    CyParam.Line.color =. CyParam.color
                ]
            |> CyGraph.withLayout (Layout.initCose <| Layout.LayoutOptions.Cose(ComponentSpacing = 40, EdgeElasticity = 100))
            |> CyGraph.withSize(1800, 800)

        /// Takes an OboOntology-based FGraph and returns a CyGraph according to its structure.
        let ontoGraphToFullCyGraph graph =
            toFullCyGraph 
                (sprintf "%s") 
                (fun (d : OboTerm) -> d.Name)
                (fun e -> 
                    [
                        CyParam.label <| e.ToString()
                        match e with
                        | ARCRelation.Follows -> CyParam.color "red"
                        | ARCRelation.PartOf -> CyParam.color "blue"
                        | x when x = ARCRelation.PartOf + ARCRelation.Follows -> CyParam.color "purple"
                    ]
                )
                graph

        /// Takes an ISA-based FGraph and returns a CyGraph according to its structure.
        let isaGraphToFullCyGraph (graph : FGraph<int*string,CvParam,ARCRelation>) =
            toFullCyGraph
                (fun (h,n) -> $"{h}, {n}")
                (fun (d : CvParam) -> $"{d.Name}: {d.Value |> ParamValue.getValueAsString}")
                (fun e -> 
                    [
                        CyParam.label <| e.ToString()
                        match e with
                        | ARCRelation.Follows -> CyParam.color "red"
                        | ARCRelation.PartOf -> CyParam.color "blue"
                        | x when x = ARCRelation.PartOf + ARCRelation.Follows -> CyParam.color "purple"
                    ]
                )
                graph
                |> CyGraph.withLayout(Layout.initBreadthfirst <| Layout.LayoutOptions.Cose())