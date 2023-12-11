namespace ARCExpect


open FsOboParser
open ControlledVocabulary
open ARCTokenization
open Graphoscope
open Cytoscape.NET
open FSharpAux

open InternalUtils


/// Functions for creating and working with ARC FGraphs.
module ARCGraph =

    /// Representation of the familiarity of a CvParam's CvTerm. If the CvTerm is known in, e.g., an ontology, use KnownTerm, else use UnknownTerm. ObsoleteTerm is for deprecated terms (i.e., OboTerm with `is_obsolete` = `true`).
    type TermFamiliarity =
        | KnownTerm of IParam
        | UnknownTerm of IParam
        | ObsoleteTerm of IParam
        | MisplacedTerm of IParam

    /// Takes a list of CvParams and returns the ArcGraph as an FGraph consisting of Nodes only.
    let fromCvParamListAsNodes cvpList =
        cvpList
        |> List.mapi (
            fun i cvp ->
                (i,CvBase.getCvName cvp), cvp
        )
        |> FGraph.createFromNodes<int*string,CvParam,ARCRelation> 

    ///// Returns all terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph via a given relating function.
    //let getRelatedCvParamsBy relating (cvp : CvParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
    //    relating graph[cvp.Accession]
    //    |> Seq.map (fun (id,rel) -> FGraph.findNode id graph, rel)
    //    |> Seq.map (fun ((id,t),r) -> id, t, r)

    ///// Returns all related terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph.
    //let getRelatedCvParams (cvp : CvParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
    //    getRelatedCvParamsBy FContext.neighbours cvp graph

    ///// Returns all succeeding terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph.
    //let getSucceedingCvParams (cvp : CvParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
    //    getRelatedCvParamsBy FContext.successors cvp graph

    ///// Returns all preceding terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph.
    //let getPrecedingCvParams (cvp : CvParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
    //    getRelatedCvParamsBy FContext.predecessors cvp graph

    ///// Checks is a given current CvParam has a given ArcRelation to a given prior CvParam by using a given ontology graph.
    //let hasRelationTo onto (relation : ARCRelation) currentCvp (priorCvp : CvParam) =
    //    getRelatedCvParams currentCvp onto
    //    |> Seq.exists (fun (id,t,r) -> id = priorCvp.Accession && r.HasFlag relation)

    ///// Checks is a given current CvParam has a follows relationship to a given prior CvParam by using a given ontology graph.
    //let hasFollowsTo onto currentCvp priorCvp =
    //    hasRelationTo onto ARCRelation.Follows currentCvp priorCvp

    ///// Checks is a given current CvParam has a part_of relationship to a given prior CvParam by using a given OboOntology.
    //let hasPartOfTo onto currentCvp priorCvp =
    //    hasRelationTo onto ARCRelation.PartOf currentCvp priorCvp

    ///// Checks if 2 given CvParams share the same ARCRelation to the same other term.
    //let equalsRelation onto (relation : ARCRelation) cvp1 cvp2 =
    //    let relTermsCvp1 = getRelatedCvParams cvp1 onto |> Seq.filter (fun (id,t,r) -> r.HasFlag relation)
    //    let relTermsCvp2 = getRelatedCvParams cvp2 onto |> Seq.filter (fun (id,t,r) -> r.HasFlag relation)
    //    relTermsCvp1
    //    |> Seq.exists (
    //        fun (id1,t1,r1) -> 
    //            relTermsCvp2
    //            |> Seq.exists (
    //                fun (id2,t2,r2) -> 
    //                    t1 = t2 && r2.HasFlag relation && r1.HasFlag relation
    //            )
    //    )

    /// Returns all terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph via a given relating function.
    let getRelatedIParamsBy relating (ip : IParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
        //relating graph[ip.Accession]
        printfn $"{graph[ip.Name]}"
        relating graph[ip.Name]
        |> Seq.map (fun (id,rel) -> FGraph.findNode id graph, rel)
        |> Seq.map (fun ((id,t),r) -> id, t, r)

    /// Returns all related terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph.
    let getRelatedIParams (ip : IParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
        getRelatedIParamsBy FContext.neighbours ip graph

    /// Returns all succeeding terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph.
    let getSucceedingCvParams (ip : IParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
        getRelatedIParamsBy FContext.successors ip graph

    /// Returns all preceding terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph.
    let getPrecedingCvParams (ip : IParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
        getRelatedIParamsBy FContext.predecessors ip graph

    /// Checks is a given current CvParam has a given ArcRelation to a given prior CvParam by using a given ontology graph.
    let hasRelationTo onto (relation : ARCRelation) currentIp (priorIp : IParam) =
        getSucceedingCvParams currentIp onto
        //|> Seq.exists (fun (id,t,r) -> id = priorIp.Accession && r.HasFlag relation)
        |> Seq.exists (fun (id,t,r) -> id = priorIp.Name && r.HasFlag relation)

    /// Checks is a given current CvParam has a follows relationship to a given prior CvParam by using a given ontology graph.
    let hasFollowsTo onto currentIp priorIp =
        hasRelationTo onto ARCRelation.Follows currentIp priorIp

    /// Checks is a given current CvParam has a part_of relationship to a given prior CvParam by using a given OboOntology.
    let hasPartOfTo onto currentIp priorIp =
        hasRelationTo onto ARCRelation.PartOf currentIp priorIp

    ///// Takes an ontology-based FGraph and returns a seq of OboTerms that are endpoints. Endpoints are OboTerms that don't have the given ArcRelation pointing at them.
    //let getEndpointsBy (arcRelation : ARCRelation) (onto : FGraph<string,OboTerm,ARCRelation>) =
    //    onto.Values
    //    |> Seq.map (fun c -> c |> fun (id,t,e) -> t, FContext.predecessors c)
    //    |> Seq.map (fun (t,p) -> t, p |> Seq.filter (fun (id,r) -> r.HasFlag arcRelation))
    //    |> Seq.choose (fun (t,p) -> if Seq.length p = 0 then Some t else None)

    ///// Takes an ontology-based FGraph and returns a seq of OboTerms that are endpoints. Endpoints are OboTerms that have no part_of relation pointing at them.
    //let getPartOfEndpoints (onto : FGraph<string,OboTerm,ARCRelation>) =
    //    getEndpointsBy ARCRelation.PartOf onto

    ///// Takes an OboTerm seq of endpoints (that is, any term without part_of predecessors) and filters a list of CvParams where every CvParam that is an endpoint is excluded.
    //let deletePartOfEndpointSectionKeys (ontoEndpoints : OboTerm seq) (cvParams : CvParam list) =
    //    cvParams
    //    |> List.filter (
    //        fun cvp ->
    //            ontoEndpoints
    //            |> Seq.exists (
    //                fun t ->
    //                    t.Name = cvp.Name &&
    //                    t.Id = cvp.Accession &&
    //                    CvParam.getValueAsTerm cvp |> fun cvt -> cvt.Accession = "AGMO:00000001" && cvt.Name = "Metadata Section Key"
    //            )
    //            |> not
    //    )

    /// Returns all terms that are present in the given ontology but don't occur in the given CvParam list as CvParams.
    let getMissingTerms (onto : OboOntology) (ips : IParam seq) =
        onto.Terms
        |> Seq.choose (
            fun o -> 
                if o.IsObsolete then None
                else 
                    let cvtObo = OboTerm.toCvTerm o
                    if not (ips |> Seq.exists (fun e -> Param.getTerm e = cvtObo)) then
                        Some (CvParam(cvtObo, Value "<missing>") :> IParam)
                    else None
        )

    /// Takes an OboOntology and a list of IParams and returns the list with all OboTerms that are missing in the list appended as empty-value IParams.
    let addMissingTerms onto ips =
        let missingTerms = getMissingTerms onto ips
        Seq.append ips missingTerms

    /// Groups the given IParams by their name and groups them together.
    let groupTerms (ips : IParam seq) =
        ips |> Seq.groupBy (fun ip -> ip.Name)       // if erroring: change to `.Accession`

    ///// Checks if a given CvParam is the header of a given OboTerm sequence.
    //let isHeader (ontoEndpoints : OboTerm seq) (cvp : CvParam) =
    //    ontoEndpoints
    //    |> Seq.exists (fun t -> t.Name = cvp.Name && t.Id = cvp.Accession)
    //    |> not

    /// Checks if a given IParam is a header term in a given OboOntology.
    let isHeader (ontoGraph : FGraph<string,OboTerm,ARCRelation>) ip =
        ontoGraph.Keys
        |> Seq.choose (
            fun k -> 
                let hasPartOfs = 
                    FContext.predecessors ontoGraph[k] 
                    |> Seq.filter (fun (nk,ed) -> ed = ARCRelation.PartOf) 
                    |> Seq.length > 0
                if hasPartOfs then
                    Some (ontoGraph[k] |> fun (p,nd,s) -> nd)
                else None
            )
        |> Seq.exists (fun term -> OboTerm.toCvTerm term = Param.getTerm ip)

    ///// Takes an ontology FGraph and a given CvParam and creates a CvParam based on a CvTerm that has the "Follows" relation to the given CvParam's term. The created CvParam's value is an empty string.
    //let createEmptyPriorFollowsCvParam onto cvp =
    //    getSucceedingCvParams cvp onto
    //    |> Seq.pick (
    //        fun (id,t,r) -> 
    //            if r.HasFlag ARCRelation.Follows then 
    //                let rowParam = CvParam(Address.row, ParamValue.Value (Param.getValueAsInt cvp["Row"] - 1))
    //                let colParam = CvParam(Address.column, ParamValue.Value (Param.getValueAsInt cvp["Column"]))
    //                let wsParam = CvParam(Address.worksheet, ParamValue.Value (Param.getValueAsString cvp["Worksheet"]))
    //                Some (CvParam(OboTerm.toCvTerm t, ParamValue.Value "", [rowParam; colParam; wsParam]))
    //            else None
    //    )

    ///// Takes an ontology FGraph and a given CvParam and creates a CvParam based on the CvTerm that the given CvParam's term is related to via the "Follows" relation. The created CvParam's value is an empty string.
    //let createEmptySubsequentFollowsCvParam onto cvp =
    //    getPrecedingCvParams cvp onto
    //    |> Seq.pick (
    //        fun (id,t,r) -> 
    //            if r.HasFlag ARCRelation.Follows then 
    //                let rowParam = CvParam(Address.row, ParamValue.Value (Param.getValueAsInt cvp["Row"] + 1))
    //                let colParam = CvParam(Address.column, ParamValue.Value (Param.getValueAsInt cvp["Column"]))
    //                let wsParam = CvParam(Address.worksheet, ParamValue.Value (Param.getValueAsString cvp["Worksheet"]))
    //                Some (CvParam(OboTerm.toCvTerm t, ParamValue.Value "", [rowParam; colParam; wsParam]))
    //            else None
    //    )

    ///// Takes an ISA-based ontology in the form of an FGraph and a list of CvParams and creates an FGraph based on a section header's "follows" and "part_of" relations.
    //let constructSubgraph isaOntology (cvParams : CvParam list) =

    //    printfn "Start constructSubgraph with CvPs: %A" cvParams

    //    let nextToSectionHeader currentCvp priorCvp =
    //        hasPartOfTo isaOntology currentCvp priorCvp
    //    let follows currentCvp priorCvp =
    //        hasFollowsTo isaOntology currentCvp priorCvp

    //    let isaGraph = FGraph.empty<int*string,CvParam,ARCRelation>

    //    let rec loop (tokens : CvParam list) (stash : CvParam list) (prior : CvParam) parent =
    //        match tokens with
    //        | h :: t ->
    //            match t with
    //            | [] ->
    //                match stash with
    //                | [] ->
    //                    printfn "done via empty stash!"
    //                    ()
    //                | _ ->
    //                    //printfn $"case new section header: h: {h.Name}, prior: {prior.Name}"
    //                    loop (h :: stash |> List.rev |> List.tail) t (stash |> List.rev |> List.head) parent
    //            | _ ->
    //                match follows h prior with
    //                | true ->
    //                    //printfn "tokensList is %A" (tokens |> List.map (fun (cvp : CvParam) -> $"{cvp.Name}: {cvp.Value |> ParamValue.getValueAsString}"))
    //                    match nextToSectionHeader h prior with
    //                    | true ->
    //                        //printfn $"case first term after section header: h: {h.Name}, prior: {prior.Name}"
    //                        FGraph.addElement (hash h,h.Name) h (hash prior,prior.Name) prior (ARCRelation.PartOf + ARCRelation.Follows) isaGraph |> ignore
    //                        printfn $"case first term after section header: h: {h.Name}, prior: {prior.Name}"
    //                        FGraph.addElement (hash h,h.Name) h (hash prior,prior.Name) prior (ARCRelation.PartOf + ARCRelation.Follows) isaGraph |> ignore
    //                        loop t (prior :: stash) h h
    //                    | false ->
    //                        //printfn $"case new term: h: {h.Name}, prior: {prior.Name}"
    //                        FGraph.addElement (hash h,h.Name) h (hash parent,parent.Name) parent ARCRelation.Follows isaGraph |> ignore
    //                        printfn $"case new term: h: {h.Name}, prior: {prior.Name}"
    //                        FGraph.addElement (hash h,h.Name) h (hash parent,parent.Name) parent ARCRelation.Follows isaGraph |> ignore
    //                        loop t stash h h
    //                | false ->
    //                    match CvParam.equalsTerm (CvParam.getTerm h) prior with
    //                    | true ->
    //                        //printfn $"case same term: h: {h.Name}, prior: {prior.Name}"
    //                        loop t (h :: stash) h parent
    //                    | false ->
    //                        //printfn $"case term missing: h: {h.Name}, prior: {prior.Name}"
    //                        let missingTerm = createEmptyPriorFollowsCvParam isaOntology h
    //                        loop (missingTerm :: h :: t) stash prior parent
    //        | [] -> 
    //            printfn "done via empty tokensList! (should not happen...)"
    //            ()

    //    FGraph.addNode (hash cvParams.Head,cvParams.Head.Name) cvParams.Head isaGraph |> ignore
    //    loop cvParams.Tail [] cvParams.Head cvParams.Head
    //    isaGraph

    ///// Takes on ISA-based ontology FGraph and a structural FGraph and closes all loose ends (i.e., creating connected nodes to such nodes that should have a Follows ArcRelation and share the same PartOf ArcRelation) of the latter according to the ontology graph.
    //let completeOpenEnds onto (graph : FGraph<(int * string),CvParam,ARCRelation>) =

    //    let kvs = List.zip (List.ofSeq graph.Keys) (List.ofSeq graph.Values)
    //    let newGraph = 
    //        FGraph.toSeq graph
    //        |> Seq.fold (fun acc (nk1,nd1,nk2,nd2,e) -> FGraph.addElement nk1 nd1 nk2 nd2 e acc) FGraph.empty 

    //    let rec loop (input : ((int * string) * FContext<(int * string),CvParam,ARCRelation>) list) =
    //        //printfn "inputL: %A" input.Length
    //        match input with
    //        | (nk1,c) :: t ->
    //            //printfn "pred: %A" (FContext.predecessors c)
    //            if FContext.predecessors c |> Seq.isEmpty then 
    //                //printfn "nk1: %A" nk1
    //                c
    //                |> fun (p,nd1,s) ->
    //                    let newS = createEmptySubsequentFollowsCvParam onto nd1
    //                    //printfn "newS: %A" newS
    //                    if equalsRelation onto ARCRelation.PartOf nd1 newS then
    //                        //printfn "addEle\n" 
    //                        let newSnk = hash newS, newS.Name
    //                        //printfn "newSnk: %A" newSnk
    //                        FGraph.addElement newSnk newS nk1 nd1 ARCRelation.Follows newGraph
    //                        |> ignore
    //                        let newSnkc = newGraph[newSnk]
    //                        let newT = (newSnk, newSnkc) :: t
    //                        //printfn "newT: %A" newT
    //                        loop newT
    //                    else 
    //                        //printfn "no addEle\n"
    //                        loop t
    //            else loop t
    //        | [] -> (*printfn "end";*) ()
    //    loop kvs

    //    newGraph

    ///// Takes a seq of OboTerms that are part_of endpoints and a list of CvParams and returns the CvParams grouped into lists of sections.
    //let groupWhenHeader partOfEndpoints (cvps : CvParam list) =
    //    cvps
    //    |> List.groupWhen (isHeader partOfEndpoints)

    ///// Takes an ISA-based ontology FGraph, an XLSX parsing function and a path to an XLSX file and returns a seq of section-based ISA-structured subgraphs.
    ///// 
    ///// `xlsxParsing` can be any of `Investigation.parseMetadataSheetFromFile`, `Study.parseMetadataSheetFromFile`, or `Assay.parseMetadataSheetFromFile`.
    //let fromXlsxFile onto (xlsxParsing : string -> IParam list) xlsxPath =
    //    let endpoints = getPartOfEndpoints onto
    //    let cvps = 
    //        xlsxParsing xlsxPath 
    //        |> List.choose (Param.tryCvParam)
    //        |> deletePartOfEndpointSectionKeys endpoints
    //        |> groupWhenHeader endpoints
    //    cvps
    //    |> Seq.map (
    //        constructSubgraph onto 
    //        >> completeOpenEnds onto
    //    )

    /// Checks if there are missing terms in a given seq of IParams by using a given ontology-based FGraph and adds them if so. A term is defined as missing if it has a part_of relation to the seq's head term and is not present in the seq's tail.
    let addMissingTermsInGroup (ontoGraph : FGraph<string,OboTerm,ARCRelation>) (ips : IParam seq) =
        let header = Seq.head ips
        let ipsTail = Seq.tail ips
        let headerChildren =
            ontoGraph[header.Name]
            |> FContext.predecessors
            |> Seq.choose (
                fun (n,e) -> 
                    if e.HasFlag ARCRelation.PartOf then 
                        ontoGraph[n]
                        |> fun (p,nd,s) ->
                            if nd.IsObsolete then None
                            else Some (OboTerm.toCvTerm nd)
                    else None
            )
        let missingParams =
            headerChildren
            |> Seq.choose (
                fun cvt ->
                    let cond = Seq.exists (fun ip -> Param.getTerm ip = cvt) ipsTail
                    if cond then None
                    else Some (CvParam(cvt, "") :> IParam)
            )
        Seq.append ips missingParams

    /// Checks if a given IParam has a part_of relation to a given header term using an ontology-based FGraph.
    let isPartOfHeader (header : IParam) (ontoGraph : FGraph<string,OboTerm,ARCRelation>) (ip : IParam) =
        ontoGraph[ip.Name]     // change to `.Accession` if required
        |> FContext.successors
        |> Seq.exists (fun (nk,e) -> nk = header.Name && e.HasFlag ARCRelation.PartOf)      // change to `.Accession` if required

    /// Checks if the given IParam contains an obsolete term using a given OboOntology.
    let isObsoleteTerm (onto : OboOntology) (ip : IParam) =
        onto.Terms
        |> Seq.exists (fun o -> o.IsObsolete && OboTerm.toCvTerm o = Param.getTerm ip)

    /// Returns the TermFamiliarity's IParam value.
    let deconstructTf tf =
        match tf with
        | KnownTerm     ip -> ip
        | UnknownTerm   ip -> ip
        | MisplacedTerm ip -> ip
        | ObsoleteTerm  ip -> ip

    /// Takes a seq of grouped IParams and tags them according to their TermFamiliarity using a given OboOntology.
    let matchTerms (onto : OboOntology) (gips : (string * IParam seq) seq) =
        let ontoGraph = OboGraph.ontologyToFGraphByName onto    // make this instead a parameter when facing performance issues!
        let header = Seq.head gips |> snd |> Seq.head
        //printfn $"header: {header.Name}"
        gips
        |> Seq.mapi (
            fun i (n,ips) ->
                if i = 0 then n, seq {KnownTerm header}
                else
                    //printfn $"ip: {(ips |> Seq.head).Name}"
                    if ips |> Seq.exists (fun ip -> Param.tryUserParam ip |> Option.isSome) then n, ips |> Seq.map UnknownTerm
                    elif ips |> Seq.exists (fun ip -> isObsoleteTerm onto ip) then n, ips |> Seq.map ObsoleteTerm
                    elif ips |> Seq.exists (fun ip -> isPartOfHeader header ontoGraph ip) then n, ips |> Seq.map KnownTerm
                    else n, ips |> Seq.map MisplacedTerm
        )

    /// Takes an ontology-based FGraph and a seq of termname * matched IParams to create an intermediate subgraph out of it. This subgraph consists of a chain of nodes that have their termname as nodekey and their IParam seq as nodedata. The nodes are ordered by the follows-relationship taken from the ontology-based FGraph.
    let constructIntermediateMetadataSubgraph (ontoGraph : FGraph<string,OboTerm,ARCRelation>) (ips : (string * TermFamiliarity seq) seq) =
        let rec loop (section : (string * TermFamiliarity seq) list) (stash : (string * TermFamiliarity seq) list) (priorParams : string * IParam seq) (graph : FGraph<string,IParam seq,ARCRelation>) =
            //printfn "next round"
            match section with
            | [] -> 
                //printfn "section empty"
                //match stash with
                //| [] -> 
                //    printfn "stash empty"
                //    graph, stash    // if section and stash are empty, return graph and empty stash
                //| _ ->
                //    printfn "stash not empty"
                //    if List.forall (fun (sn,stf) -> match Seq.head stf with MisplacedTerm _ -> true | _ -> false) stash then 
                //        printfn "only MisplacedTerms"
                //        graph, stash   // if section is empty and stash only has MisplacedTerms, return graph and stash
                //    else 
                //        printfn "some non-MisplacedTerms"
                //        loop stash [] priorParams graph     // else take stash as section and continue
                graph, stash
            | (hn,hts) :: t ->
                //printfn "section not empty"
                match Seq.head hts with
                | UnknownTerm ip ->     // if UnknownTerm then add with Unknown relation to prior node
                    //printfn "UnknownTerm"
                    FGraph.addElement hn (Seq.map deconstructTf hts) (fst priorParams) (snd priorParams) ARCRelation.Unknown graph
                    |> loop t stash priorParams
                | KnownTerm ip ->
                    //printfn "KnownTerm"
                    let priorName,priorIps = priorParams
                    if hasFollowsTo ontoGraph ip (Seq.head priorIps) then   //
                        //printfn "has follows"
                        let hips = hts |> Seq.map deconstructTf
                        FGraph.addElement hn hips priorName priorIps ARCRelation.Follows graph
                        |> loop t stash (hn, hips)
                    else
                        //printfn "has no follows"
                        loop t ((hn,hts) :: stash) priorParams graph
                | ObsoleteTerm ip ->
                    //printfn "ObsoleteTerm"
                    let priorName,priorIps = priorParams
                    if hasFollowsTo ontoGraph ip (Seq.head priorIps) then
                        //printfn "has follows"
                        let hips = hts |> Seq.map deconstructTf
                        FGraph.addElement hn hips priorName priorIps (ARCRelation.Follows + ARCRelation.Obsolete) graph
                        |> loop t stash (hn, hips)
                    else
                        //printfn "has no follows"
                        loop t ((hn,hts) :: stash) priorParams graph
                | MisplacedTerm ip ->
                    //printfn "MisplacedTerm"
                    FGraph.addElement hn (Seq.map deconstructTf hts) (fst priorParams) (snd priorParams) ARCRelation.Misplaced graph
                    |> loop t stash priorParams
        let ipsList = Seq.toList ips
        loop ipsList.Tail [] (fst ipsList.Head, (snd >> Seq.map deconstructTf) ipsList.Head) FGraph.empty<string,IParam seq,ARCRelation>

    /// Takes a subgraph and adds empty IParams of the respective CvTerm to the nodedata if it is shorter than the longest IParam seq of any nodedata so that all IParam seqs have the same amount of items. Ignores the header.
    let addEmptyIpsToNodeData (subgraph : FGraph<string,IParam seq,ARCRelation>) =
        let longestChainLength = 
            FGraph.getNodes subgraph
            |> Seq.maxBy (snd >> Seq.length)
            |> snd
            |> Seq.length
        let header = FGraph.getTopNodeKey subgraph
        subgraph.Keys   // .mapNodes would be nicer...
        |> Seq.iter (
            fun nk ->
                if nk <> header then
                    let nd = subgraph[nk] |> fun (p,nd,s) -> nd
                    let currLength = Seq.length nd
                    if currLength < longestChainLength then
                        let emptyIps = Seq.init (longestChainLength - currLength) (fun _ -> CvParam(Seq.head nd |> Param.getTerm, "<empty>") :> IParam)
                        FGraph.setNodeData nk (Seq.append nd emptyIps) subgraph
                        |> ignore
        )
        subgraph

    /// Splits the nodedata of a given intermediate subgraph into single nodes. Nodekey changes from name to name * number (of occurence), 0-based.
    let splitMetadataSubgraph (subgraph : FGraph<string,IParam seq,ARCRelation>) =
        let header = FGraph.getTopNodeKey subgraph
        //printfn $"header: {header}"
        let newGraph =
            subgraph.Keys
            |> Seq.fold (
                fun g nk ->
                    if nk = header then
                        let nd = FGraph.getNodeData nk subgraph |> Seq.head
                        FGraph.addNode (nk,0) nd g
                    else
                        let nds = FGraph.getNodeData nk subgraph
                        nds
                        |> Seq.foldi (
                            fun i g2 nd ->
                                FGraph.addNode (nk,i) nd g2 
                        ) g
            ) FGraph.empty<string * int,IParam,ARCRelation>
        newGraph.Keys
        |> Seq.iter (
            fun (nk,i) ->
                //printfn $"nk: {nk}, i: {i}"
                let succs = FContext.successors subgraph[nk]
                succs
                |> Seq.iter (
                    fun (nk2,e) ->
                        if nk2 = header then
                            //printfn "edge for header"
                            FGraph.addEdge (nk,i) (nk2,0) e newGraph
                        else
                            //printfn "edge for non-header"
                            FGraph.addEdge (nk,i) (nk2,i) e newGraph
                        |> ignore
                )
        )
        newGraph

    /// Takes a metadata subgraph and returns its content as a list of flat list in the form of (name * number) * nodedata. The inner list has their items grouped by the number.
    let metadataSubgraphToList (subgraph : FGraph<string * int,IParam,ARCRelation>) =
        let headerN, headerI = FGraph.getTopNodeKey subgraph
        let chainMaxNo = subgraph.Keys |> Seq.maxBy snd |> snd
        Seq.init (chainMaxNo + 1) (fun i ->
            subgraph.Keys
            |> Seq.choose (
                fun (nk,i2) ->
                    if nk = headerN then
                        ((headerN, headerI), FGraph.getNodeData (nk,0) subgraph)
                        |> Some
                    elif i = i2 then
                        ((nk, i), FGraph.getNodeData (nk,i) subgraph)
                        |> Some
                    else None
            )
        )


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
            //|> CyGraph.withLayout (Layout.initCose <| Layout.LayoutOptions.Cose(ComponentSpacing = 40, EdgeElasticity = 100))
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
        let isaGraphToFullCyGraph (graph : FGraph<string,IParam,ARCRelation>) =
            toFullCyGraph
                //(fun (h,n) -> $"{h}, {n}")    // when using hash * accession or hash * name
                id      // when using only accession or name
                (fun (d : IParam) -> $"{d.Name}: {d.Value |> ParamValue.getValueAsString}")
                (fun e -> 
                    [
                        CyParam.label <| e.ToString()
                        match e with
                        | ARCRelation.Follows -> CyParam.color "red"
                        | ARCRelation.PartOf -> CyParam.color "blue"
                        | x when x = ARCRelation.PartOf + ARCRelation.Follows -> CyParam.color "purple"
                        | ARCRelation.IsA -> CyParam.color "lightblue"
                        | ARCRelation.Misplaced -> CyParam.color "pink"
                        | ARCRelation.Obsolete -> CyParam.color "yellow"
                        | ARCRelation.Unknown -> CyParam.color "black"
                        | x when x = ARCRelation.Obsolete + ARCRelation.Follows -> CyParam.color "orange"
                        | ARCRelation.HasA -> CyParam.color "brown"
                        | _ -> CyParam.color "white"
                    ]
                )
                graph
                |> CyGraph.withLayout(Layout.initBreadthfirst <| Layout.LayoutOptions.Cose())

        /// Takes an ISA-based FGraph and returns a CyGraph according to its structure.
        let isaIntermediateGraphToFullCyGraph (graph : FGraph<string,IParam seq,ARCRelation>) =
            toFullCyGraph
                //(fun (h,n) -> $"{h}, {n}")    // when using hash * accession or hash * name
                id      // when using only accession or name
                (fun (d : IParam seq) -> $"""{(Seq.head d).Name}: {(Seq.map (fun (lil : IParam) -> lil.Value |> ParamValue.getValueAsString) d) |> String.concat "; "}""")
                (fun e -> 
                    [
                        CyParam.label <| e.ToString()
                        match e with
                        | ARCRelation.Follows -> CyParam.color "red"
                        | ARCRelation.PartOf -> CyParam.color "blue"
                        | x when x = ARCRelation.PartOf + ARCRelation.Follows -> CyParam.color "purple"
                        | ARCRelation.IsA -> CyParam.color "lightblue"
                        | ARCRelation.Misplaced -> CyParam.color "pink"
                        | ARCRelation.Obsolete -> CyParam.color "yellow"
                        | ARCRelation.Unknown -> CyParam.color "black"
                        | x when x = ARCRelation.Obsolete + ARCRelation.Follows -> CyParam.color "orange"
                        | ARCRelation.HasA -> CyParam.color "brown"
                        | _ -> CyParam.color "white"
                    ]
                )
                graph
                |> CyGraph.withLayout(Layout.initBreadthfirst <| Layout.LayoutOptions.Cose())

        /// Takes an ISA-based FGraph and returns a CyGraph according to its structure.
        let isaSplitGraphToFullCyGraph (graph : FGraph<string * int,IParam,ARCRelation>) =
            toFullCyGraph
                //(fun (h,n) -> $"{h}, {n}")    // when using hash * accession or hash * name
                (fun (nk,i) -> $"{nk}, {i}")
                (fun (d : IParam) -> $"{d.Name}: {d.Value |> ParamValue.getValueAsString}")
                (fun e -> 
                    [
                        CyParam.label <| e.ToString()
                        match e with
                        | ARCRelation.Follows -> CyParam.color "red"
                        | ARCRelation.PartOf -> CyParam.color "blue"
                        | x when x = ARCRelation.PartOf + ARCRelation.Follows -> CyParam.color "purple"
                        | ARCRelation.IsA -> CyParam.color "lightblue"
                        | ARCRelation.Misplaced -> CyParam.color "pink"
                        | ARCRelation.Obsolete -> CyParam.color "yellow"
                        | ARCRelation.Unknown -> CyParam.color "black"
                        | x when x = ARCRelation.Obsolete + ARCRelation.Follows -> CyParam.color "orange"
                        | ARCRelation.HasA -> CyParam.color "brown"
                        | _ -> CyParam.color "white"
                    ]
                )
                graph
                |> CyGraph.withLayout(Layout.initBreadthfirst <| Layout.LayoutOptions.Cose())