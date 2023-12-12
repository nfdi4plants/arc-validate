#I "../ARCTokenization/src/ARCTokenization/bin/Debug/netstandard2.0"
#I "../ARCTokenization/src/ARCTokenization/bin/Release/netstandard2.0"
#r "ARCTokenization.dll"
#r "ControlledVocabulary.dll"
#I "src/ARCExpect/bin/Debug/netstandard2.0"
#I "src/ARCExpect/bin/Release/netstandard2.0"
#r "ARCExpect.dll"

//#r "nuget: ARCTokenization"
#r "nuget: Expecto"
#r "nuget: FSharpAux, 1.1.0"
#r "nuget: Graphoscope"
#r "nuget: Cytoscape.NET"
#r "nuget: FsOboParser, 0.3.0"
#r "nuget: FsSpreadsheet.ExcelIO, 4.1.0"


open Expecto
open ControlledVocabulary
open ARCTokenization
open FSharpAux
//open ArcValidation.OntologyHelperFunctions
//open ArcValidation.ErrorMessage
open Graphoscope
open FsOboParser
open Cytoscape.NET

open ARCExpect
open ARCExpect.OboGraph
open ARCExpect.ARCGraph
open ARCExpect.ARCGraph.Visualization

open System.Collections.Generic
open System.Text.RegularExpressions


//// from internal module copypasted

//open Impl

//let performTest test =
//    let w = System.Diagnostics.Stopwatch()
//    w.Start()
//    evalTests Tests.defaultConfig test
//    |> Async.RunSynchronously
//    |> fun r -> 
//        w.Stop()
//        {
//            results = r
//            duration = w.Elapsed
//            maxMemory = 0L
//            memoryLimit = 0L
//            timedOut = []
//        }

let paramse = ARCTokenization.Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation.xlsx"

//paramse |> List.map (fun p -> p.ToString() |> String.contains "CvParam") |> List.reduce (&&)
//paramse |> List.iter (fun p -> printfn "%A" <| p.GetType().ToString())
//paramse |> List.iter (fun p -> printfn "%A" <| (p.Value |> ParamValue.getValueAsString))
//paramse |> List.iter (fun p -> printfn "%A" <| p.Name)

//let cvparamse = paramse |> List.map (CvParam.tryCvParam >> Option.get)
//let cvparamse = 
//    paramse 
//    |> List.map (
//        fun p -> 
//            match CvParam.tryCvParam p with
//            | Some cvp -> cvp
//            | None -> CvParam(p.ID, p.Name, p.RefUri, p.Value, p :?> CvAttributeCollection)
//    )

let cvparamse = paramse |> List.map (Param.tryCvParam >> Option.get)

//let fromCvParamList cvpList =
//    cvpList
//    |> List.mapi (
//        fun i cvp ->
//            (i,CvBase.getCvName cvp), cvp
//    )
//    |> FGraph.createFromNodes<int*string,CvParam,Relation> 

//let invesContentGraph = fromCvParamList cvparamse

let onto = ARCTokenization.Terms.InvestigationMetadata.ontology

//let tans = cvparamse |> List.map CvParam.getCvAccession

//let assTerms = tans |> List.choose (fun tan -> obo.Terms |> List.tryFind (fun term -> term.Id = tan))
//assTerms |> List.fold (fun acc y -> acc && Option.isSome y) true

//let assTermsRelships = assTerms |> List.collect (fun x -> OboOntology.getRelatedTerms x obo)

//toRelation "part_of" + toRelation "has_a" + toRelation "follows"
//toRelation "part_of" ||| toRelation "has_a" ||| toRelation "follows"

//let assTermsRels = assTermsRelships |> List.map (fun (o1,rs,o2) -> o1, toRelation rs, o2)

//invesContentGraph.Keys |> Seq.head
//invesContentGraph.Values |> Seq.head

//let ontoGraph = ontologyToFGraph onto

//ontoGraph |> printGraph (fun x -> x.Name)

//ontoGraphToFullCyGraph ontoGraph |> CyGraph.show

let ontologyToFGraphByName (onto : OboOntology) =
    OboOntology.getRelations onto
    |> List.fold (
        fun acc tr ->
            match tr with
            | Empty st -> FGraph.addNode st.Name st acc
            | TargetMissing (rel,st) -> FGraph.addNode st.Name st acc
            | Target (rel,st,tt) -> FGraph.addElement st.Name st tt.Name tt (ARCRelation.toARCRelation rel) acc
    ) FGraph.empty<string,OboTerm,ARCRelation>

let ontoGraph = ontologyToFGraphByName onto


// NEW METADATA GRAPH CREATION FUNCTION(S)

// input: OboGraph, CvParam list


/// Returns the respective Term Source Ref of a given ID (= Term Accession Number).
let getRef id =
    String.takeWhile ((<>) ':') id


/// Returns all IParams whose terms are not present in the given ontology but occur in the given CvParam list.
let getUnknownTerms (onto : OboOntology) (ips : IParam seq) =
    ips
    |> Seq.filter (
        fun ip -> 
            onto.Terms 
            |> Seq.exists (fun o -> OboTerm.toCvTerm o = Param.getTerm ip)
            |> not
    )

/// Returns all IParams whose terms have the `is_obsolete` tag in the given ontology.
let getObsoleteTerms (onto : OboOntology) (ips : IParam seq) =
    ips
    |> Seq.filter (
        fun ip ->
            onto.Terms
            |> Seq.exists (fun o -> o.IsObsolete && OboTerm.toCvTerm o = Param.getTerm ip)
    )

let obsos = getObsoleteTerms onto paramse

/// Returns all terms that are present in the given ontology but don't occur in the given CvParam list as CvParams.
let getMissingTerms (onto : OboOntology) (ips : IParam seq) =
    onto.Terms
    |> Seq.choose (
        fun o -> 
            if o.IsObsolete then None
            else 
                let cvtObo = OboTerm.toCvTerm o
                if not (ips |> Seq.exists (fun e -> Param.getTerm e = cvtObo)) then
                    Some (CvParam(cvtObo, Value "") :> IParam)
                else None
    )


/// Representation of the familiarity of a CvParam's CvTerm. If the CvTerm is known in, e.g., an ontology, use KnownTerm, else use UnknownTerm. ObsoleteTerm is for deprecated terms (i.e., OboTerm with `is_obsolete` = `true`).
type TermFamiliarity =
    | KnownTerm of IParam
    | UnknownTerm of IParam
    | ObsoleteTerm of IParam
    | MisplacedTerm of IParam


/// Takes an OboOntology and a list of IParams and returns the list with all IParams marked as known in the given ontology, unknown, or obsolete.
let markTerms onto ips =
    let unknownTerms = getUnknownTerms onto ips
    let obsoleteTerms = getObsoleteTerms onto ips
    ips
    |> Seq.map (
        fun ip -> 
            match Seq.contains ip unknownTerms, Seq.contains ip obsoleteTerms with
            | true, _ -> UnknownTerm ip
            | _, true -> ObsoleteTerm ip
            | _ -> KnownTerm ip
    )

/// Takes an OboOntology and a list of CvParams and returns the list with all OboTerms that are missing in the list appended as empty-value CvParams.
let addMissingTerms onto ips =
    let missingTerms = getMissingTerms onto ips
    Seq.append ips missingTerms

/// Groups the given IParams by their name and groups them together.
let groupTerms (ips : IParam seq) =
    ips |> Seq.groupBy (fun ip -> ip.Name)       // if erroring: change to `.Accession`


let ipsAdded = addMissingTerms onto paramse
//ipsAdded |> Seq.iter (fun c -> c.Name |> printfn "%s")
//onto.Terms |> List.filter (fun o -> o.Synonyms.Length > 0)

//let ipsAggregated = aggregateTerms ipsAdded
//ipsAggregated |> Seq.iter (printfn "%A")

//let cvpsMarked = ipsAggregated |> Seq.map (fun (n,cs) -> n, markTerms onto cs)
//cvpsMarked |> Seq.iter (fun c -> match c with | KnownTerm x | ObsoleteTerm x -> () | UnknownTerm x -> printfn "%A" x)

type FGraph with

    /// Returns the nodes of a given FGraph.
    static member getNodes (graph : FGraph<'Nk,'Nd,'Ed>) =
        graph
        |> Seq.map (
           fun kvp ->
                let nodeKey = kvp.Key
                let p,nd,s = kvp.Value
                nodeKey, nd
           )


/// Returns the key of the node in a structured ontology-FGraph that has no other nodes pointing to.
let getTopNodeKey (ontoGraph : FGraph<string,OboTerm,ARCRelation>) =
    ontoGraph.Keys
    |> Seq.find (fun k -> FContext.successors ontoGraph[k] |> Seq.length = 0)

//ontoGraph[getTopNodeKey ontoGraph] |> fun (p,nd,s) -> nd

///// Creates an intermediate graph with CvParam seq as nodedata.
//let createIntermediateGraph (ontoGraph : FGraph<string,OboTerm,ArcRelation>) cvps =
//    let topNodeKey = getTopNodeKey ontoGraph
//    let rec loop inputList currentKey priorTerm outputGraph =
//        let _,oboTerm,_ = ontoGraph[currentKey]
//        let cvtObo = OboTerm.toCvTerm oboTerm

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

//isHeader ontoGraph cvparamse[2]
//isHeader ontoGraph cvparamse[5]


/// Checks if a given IParam has a part_of relation to a given header term using an ontology-based FGraph.
let isPartOfHeader (header : IParam) (ontoGraph : FGraph<string,OboTerm,ARCRelation>) (ip : IParam) =
    ontoGraph[ip.Name]     // change to `.Accession` if required
    |> FContext.successors
    |> Seq.exists (fun (nk,e) -> nk = header.Name && e.HasFlag ARCRelation.PartOf)      // change to `.Accession` if required

/// Checks if the given IParam contains an obsolete term using a given OboOntology.
let isObsoleteTerm (onto : OboOntology) (ip : IParam) =
    onto.Terms
    |> Seq.exists (fun o -> o.IsObsolete && OboTerm.toCvTerm o = Param.getTerm ip)

/// Takes a seq of grouped IParams and tags them according to their TermFamiliarity using a given OboOntology.
let matchTerms (onto : OboOntology) (gips : (string * IParam seq) seq) =
    let ontoGraph = ontologyToFGraphByName onto      // if time performance is crucial, have this as parameter instead
    let header = Seq.head gips |> snd |> Seq.head
    printfn $"header: {header.Name}"
    gips
    |> Seq.mapi (
        fun i (n,ips) ->
            if i = 0 then n, seq {KnownTerm header}
            else
                printfn $"ip: {(ips |> Seq.head).Name}"
                if ips |> Seq.exists (fun ip -> Param.tryUserParam ip |> Option.isSome) then n, ips |> Seq.map UnknownTerm
                elif ips |> Seq.exists (fun ip -> isObsoleteTerm onto ip) then n, ips |> Seq.map ObsoleteTerm
                elif ips |> Seq.exists (fun ip -> isPartOfHeader header ontoGraph ip) then n, ips |> Seq.map KnownTerm
                else n, ips |> Seq.map MisplacedTerm
    )

let partitionedIps = Seq.groupWhen (isHeader ontoGraph) paramse
//partitionedIps |> Seq.map Seq.toList |> Seq.toList
//partitionedIps |> Seq.iter (fun ips -> printfn ""; ips |> Seq.iter (fun ip -> printfn "%s" ip.Name))

let groupedIps = partitionedIps |> Seq.map groupTerms 
//groupedIps|>Seq.iter(fun ips->printfn"";ips|>Seq.iter(fun(ipN,ipEs)->printfn$"{ipN}:";ipEs|>Seq.iter(fun ip->printfn$"\t{ParamValue.getValueAsString ip.Value}")))

// deprecated: (dropped in favor of reworking matchTerms input parameter)
///// Aggregates groups of IParams together.
//let aggregateTerms (groupedIps : (string * IParam seq) seq) =
//    groupedIps
//    |> Seq.map snd
//    |> Seq.concat

//let aggregatedIps = Seq.map aggregateTerms groupedIps

//let matchedIps = aggregatedIps |> Seq.map (matchTerms onto)
let matchedIps = groupedIps |> Seq.map (matchTerms onto)
matchedIps |> Seq.head
let header = paramse.Head
isHeader ontoGraph header
let ip = paramse[2]
isPartOfHeader header ontoGraph ip
//ontoGraph[ip.Name] |> FContext.predecessors |> Seq.exists (fun (nk,e) -> printfn $"nk: {nk}\nheader: {header.Name}"; nk = header.Name)
ontoGraph[ip.Name] |> FContext.successors |> Seq.exists (fun (nk,e) -> printfn $"nk: {nk}\nheader: {header.Name}"; nk = header.Name)
onto.Terms[3]
//matchTerms onto [header; ip]
let testHead1 = groupedIps |> Seq.head
//let testHead1a = aggregatedIps |> Seq.head |> Seq.toList
let testHead1a = groupedIps |> Seq.head |> Seq.toList
groupedIps |> Seq.item 3 |> Seq.toList
matchedIps |> Seq.item 3 |> Seq.toList
matchedIps |> Seq.last |> Seq.toList

// SSSSSSSSSSSSSSSSSSSSSSSSS
// altered from ARCGraph.fs:

/// Returns all terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph via a given relating function.
let getRelatedCvParamsBy relating (ip : IParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
    relating graph[ip.Accession]
    |> Seq.map (fun (id,rel) -> FGraph.findNode id graph, rel)
    |> Seq.map (fun ((id,t),r) -> id, t, r)

/// Returns all related terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph.
let getRelatedCvParams (ip : IParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
    getRelatedCvParamsBy FContext.neighbours ip graph

/// Returns all succeeding terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph.
let getSucceedingCvParams (ip : IParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
    getRelatedCvParamsBy FContext.successors ip graph

/// Returns all preceding terms (as ID * OboTerm * ArcRelation) of a given CvParam by using a given ontology graph.
let getPrecedingCvParams (ip : IParam) (graph : FGraph<string,OboTerm,ARCRelation>) =
    getRelatedCvParamsBy FContext.predecessors ip graph

/// Checks is a given current CvParam has a given ArcRelation to a given prior CvParam by using a given ontology graph.
let hasRelationTo onto (relation : ARCRelation) currentIp (priorIp : IParam) =
    getRelatedCvParams currentIp onto
    |> Seq.exists (fun (id,t,r) -> id = priorIp.Accession && r.HasFlag relation)

/// Checks is a given current CvParam has a follows relationship to a given prior CvParam by using a given ontology graph.
let hasFollowsTo onto currentIp priorIp =
    hasRelationTo onto ARCRelation.Follows currentIp priorIp

/// Checks is a given current CvParam has a part_of relationship to a given prior CvParam by using a given OboOntology.
let hasPartOfTo onto currentIp priorIp =
    hasRelationTo onto ARCRelation.PartOf currentIp priorIp

// EEEEEEEEEEEEEEEEEEEEEEEEE

/// Returns the TermFamiliarity's IParam value.
let deconstructTf tf =
    match tf with
    | KnownTerm     ip -> ip
    | UnknownTerm   ip -> ip
    | MisplacedTerm ip -> ip
    | ObsoleteTerm  ip -> ip

let constructSubgraph (ontoGraph : FGraph<string,OboTerm,ARCRelation>) (ips : (string * TermFamiliarity seq) seq) =
    let rec loop (section : (string * TermFamiliarity seq) list) (stash : (string * TermFamiliarity seq) list) (header : IParam) (priorParams : string * IParam seq) (graph : FGraph<string,IParam seq,ARCRelation>) =
        match section with
        | (hn,hts) :: t ->
            match Seq.head hts with
            | UnknownTerm ip -> 
                FGraph.addElement hn (hts |> Seq.map deconstructTf) (fst priorParams) (snd priorParams) ARCRelation.Unknown graph
                |> loop t stash header priorParams
            | KnownTerm ip ->
                let priorName,priorIps = priorParams
                if hasFollowsTo ontoGraph ip (Seq.head priorIps) then
                    let hips = (hts |> Seq.map deconstructTf)
                    FGraph.addElement hn hips priorName priorIps ARCRelation.Follows graph
                    |> loop t stash header (hn, hips)
                else
                    loop t ()
    loop 




