#I "../ARCTokenization/src/ARCTokenization/bin/Debug/netstandard2.0"
#I "../ARCTokenization/src/ARCTokenization/bin/Release/netstandard2.0"
#r "ARCTokenization.dll"
#r "ControlledVocabulary.dll"
#I "src/ArcValidation/bin/Debug/netstandard2.0"
#I "src/ArcValidation/bin/Release/netstandard2.0"
#r "ARCValidation.dll"

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

open ArcValidation
open ArcValidation.OboGraph
open ArcValidation.ArcGraph
open ArcValidation.ArcGraph.Visualization

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

let ontoGraph = ontologyToFGraph onto

//ontoGraph |> printGraph (fun x -> x.Name)

//ontoGraphToFullCyGraph ontoGraph |> CyGraph.show


// NEW METADATA GRAPH CREATION FUNCTION(S)

// input: OboGraph, CvParam list


/// Returns the respective Term Source Ref of a given ID (= Term Accession Number).
let getRef id =
    String.takeWhile ((<>) ':') id


type OboTerm with

    member this.ToCvTerm() =
        CvTerm.create(this.Id, this.Name, getRef this.Id)

    static member toCvTerm (term : OboTerm) =
        term.ToCvTerm()


/// Returns all CvParams whose terms are not present in the given ontology but occur in the given CvParam list.
let getUnknownTerms (onto : OboOntology) (cvps : CvParam seq) =
    cvps
    |> Seq.filter (
        fun c -> 
            onto.Terms 
            |> Seq.exists (fun o -> OboTerm.toCvTerm o = CvParam.getTerm c)
            |> not
    )

/// Returns all CvParams whose terms have the `is_obsolete` tag in the given ontology.
let getObsoleteTerms (onto : OboOntology) (cvps : CvParam seq) =
    cvps
    |> Seq.filter (
        fun c ->
            onto.Terms
            |> Seq.exists (fun o -> o.IsObsolete && OboTerm.toCvTerm o = CvParam.getTerm c)
    )

let obsos = getObsoleteTerms onto cvparamse

/// Returns all terms that are present in the given ontology but don't occur in the given CvParam list as CvParams.
let getMissingTerms (onto : OboOntology) (cvps : CvParam seq) =
    onto.Terms
    |> Seq.choose (
        fun o -> 
            if o.IsObsolete then None
            else 
                let cvtObo = OboTerm.toCvTerm o
                if not (cvps |> Seq.exists (fun e -> CvParam.getTerm e = cvtObo)) then
                    Some <| CvParam(cvtObo, Value "")
                else None
    )


/// Representation of the familiarity of a CvParam's CvTerm. If the CvTerm is known in, e.g., an ontology, use KnownTerm, else use UnknownTerm. ObsoleteTerm is for deprecated terms (i.e., OboTerm with `is_obsolete` = `true`).
type TermFamiliarity =
    | KnownTerm of CvParam
    | UnknownTerm of CvParam
    | ObsoleteTerm of CvParam


/// Takes an OboOntology and a list of CvParams and returns the list with all CvParams marked as known in the given ontology, unknown, or obsolete.
let markTerms onto cvps =
    let unknownTerms = getUnknownTerms onto cvps
    let obsoleteTerms = getObsoleteTerms onto cvps
    cvps
    |> Seq.map (
        fun cvp -> 
            match Seq.contains cvp unknownTerms, Seq.contains cvp obsoleteTerms with
            | true, _ -> UnknownTerm cvp
            | _, true -> ObsoleteTerm cvp
            | _ -> KnownTerm cvp
    )

/// Takes an OboOntology and a list of CvParams and returns the list with all OboTerms that are missing in the list appended as empty-value CvParams.
let addMissingTerms onto cvps =
    let missingTerms = getMissingTerms onto cvps
    Seq.append cvps missingTerms

/// Aggregates the given CvParams by their name and groups them together.
let aggregateTerms (cvps : CvParam seq) =
    cvps |> Seq.groupBy (fun cvp -> cvp.Name)       // if erroring: change to `.Accession`


let cvpsAdded = addMissingTerms onto cvparamse
cvpsAdded |> Seq.iter (fun c -> c.Name |> printfn "%s")
onto.Terms |> List.filter (fun o -> o.Synonyms.Length > 0)

let cvpsAggregated = aggregateTerms cvpsAdded
cvpsAggregated |> Seq.iter (printfn "%A")

let cvpsMarked = cvpsAggregated |> Seq.map (fun (n,cs) -> n, markTerms onto cs)
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


/// Returns the node in a structured ontology-FGraph that has no other nodes pointing to.
let getTopNode (ontoGraph : FGraph<string,OboTerm,ArcRelation>) =
    ontoGraph.Keys
    |> Seq.find (fun k -> FContext.successors ontoGraph[k] |> Seq.length = 0)

ontoGraph[getTopNode ontoGraph] |> fun (p,nd,s) -> nd

/// Creates
let createIntermediateGraph (ontoGraph : FGraph<string,OboTerm,ArcRelation>) cvps =
    let topNode = getTopNode ontoGraph
    let rec loop inputList outputGraph =




// OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO
// Helper functions for ISA graph construction
// OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO

/// Checks if 2 given CvParams share the same part_of relationship to the same other term.
let equalsPartOf onto cvp1 cvp2 =
    equalsRelation onto ArcRelation.PartOf cvp1 cvp2

/// Checks if 2 given CvParams share the same follows relationship to the same other term.
let equalsFollows onto cvp1 cvp2 =
    equalsRelation onto ArcRelation.Follows cvp1 cvp2

//let findSectionHead onto currentCvp graph =
//    let r = getRelatedCvParams currentCvp onto |> Seq.tryPick (fun (id,t,r) -> if r.HasFlag ArcRelation.PartOf then Some id else None)
//    FGraph.findNode 

//let getPreviousFollow cvp onto subgraph =
//    let rt = getRelatedCvParams cvp onto |> Seq.filter (fun (id,t,r) -> r.HasFlag ArcRelation.Follows)

//obo.Terms[2]
//getEndpoints ontoGraph |> List.ofSeq |> List.map (fun t -> t.Name)
//|> List.ofSeq


//deleteEndpointSectionKeys (getEndpoints ontoGraph) cvparamse

//let cvparamseNoEndpointSectionKeys = deleteEndpointSectionKeys (getEndpoints ontoGraph) cvparamse

//cvparamseNoEndpointSectionKeys
//|> Seq.groupWhen (getEndpoints ontoGraph |> isHeader)
//|> Seq.toList
//|> List.item 1

// [deprecated]
//let getFollowTerm (onto : FGraph<string,OboTerm,ArcRelation>) cvp =
//    getRelatedCvParams cvp onto
//    |> Seq.map (fun (id,t,r) -> r.HasFlag ArcRelation.Follows)

//getFollowTerm ontoGraph cvparamse[1], cvparamse[1]

//OboTerm.toCvTerm (ontoGraph.Values |> Seq.head |> fun (_,t,_) -> t)

//cvparamse.[1].Attributes |> Dictionary.item "Row" 

//cvparamse[7]["Column"]
//(createEmptyFollowsCvParam ontoGraph cvparamse[8])["Column"]
//cvparamse[7]["Row"]
//(createEmptyFollowsCvParam ontoGraph cvparamse[8])["Row"]
//cvparamse[7]["Worksheet"]
//(createEmptyFollowsCvParam ontoGraph cvparamse[8])["Worksheet"]


let cvpContactsSimple = 
    Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation_ContactsOnly_Simple.xlsx"
    |> List.map (Param.toCvParam)

//let doneGraphSimple = constructSubgraph ontoGraph cvpContactsSimple
let doneGraphSimple = constructSubgraph ontoGraph (getPartOfEndpoints ontoGraph |> deletePartOfEndpointSectionKeys <| cvpContactsSimple)
doneGraphSimple |> printGraph (fun x -> $"{x.Name}: {x.Value |> ParamValue.getValueAsString}")
doneGraphSimple |> isaGraphToFullCyGraph |> CyGraph.show

let cvpContactsComplicated = 
    Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation_ContactsOnly_Complicated.xlsx"
    |> List.map (Param.toCvParam)
let cvpContactsComplicatedReassessed = deletePartOfEndpointSectionKeys (getPartOfEndpoints ontoGraph) cvpContactsComplicated

let doneGraphComplicated = constructSubgraph ontoGraph cvpContactsComplicatedReassessed
doneGraphComplicated |> printGraph (fun x -> $"{x.Name}: {x.Value |> ParamValue.getValueAsString}")
doneGraphComplicated |> isaGraphToFullCyGraph |> CyGraph.show

let wrongTermInContacts = ArcGraph.fromXlsxFile ontoGraph Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation_wrongTermInContacts.xlsx"
wrongTermInContacts |> List.ofSeq
let wrongTermInContactsF = Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation_wrongTermInContacts.xlsx"

let res0 = fromXlsxFile ontoGraph Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation.xlsx"
res0 |> Seq.head |> Visualization.isaGraphToFullCyGraph |> CyGraph.show
res0 |> Seq.item 1 |> Visualization.isaGraphToFullCyGraph |> CyGraph.withLayout (Layout.initGrid (Layout.LayoutOptions.Cose(NodeRepulsion = 500000000))) |> CyGraph.show
res0 |> Seq.iter (Visualization.isaGraphToFullCyGraph >> CyGraph.show)
res0 |> Seq.toList
res0
|> Seq.iteri (
    fun i e ->
        printfn "%i" i
        Visualization.isaGraphToFullCyGraph e
        |> ignore
)

let eps = getPartOfEndpoints ontoGraph
cvparamse 
|> deletePartOfEndpointSectionKeys eps
|> groupWhenHeader eps
|> List.map (constructSubgraph ontoGraph)
|> List.mapi (
    fun i x -> completeOpenEnds ontoGraph x
)


let res1 = fromXlsxFile (ontologyToFGraph Terms.StudyMetadata.ontology) Study.parseMetadataSheetfromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\studies\experiment1_material\isa.study.xlsx"
let res1 = fromXlsxFile (ontologyToFGraph Terms.StudyMetadata.ontology) Study.parseMetadataSheetfromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\studies\experiment1_material\isa.study_unnecessarilyFilled.xlsx"

res1 |> Seq.iter (Visualization.isaGraphToFullCyGraph >> CyGraph.show)
res1 |> Seq.last |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.toList
res1 |> Seq.head |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.item 6 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.item 7 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.item 8 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.iteri (fun i _ -> printfn "%i" i)
res1 |> Seq.item 100 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.head


let ontoGraph1 = ontologyToFGraph Terms.StudyMetadata.ontology
let endpoints = getPartOfEndpoints ontoGraph1
let cvps1a = Study.parseMetadataSheetfromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\studies\experiment1_material\isa.study_unnecessarilyFilled.xlsx" 
cvps1a.Length
let cvps1b = cvps1a |> List.choose (Param.tryCvParam)
cvps1b.Length
let cvps1c = cvps1b |> deletePartOfEndpointSectionKeys endpoints
cvps1c.Length
cvps1c |> List.iter (fun x -> printfn "%s" x.Name)
let cvps1d = cvps1c |> groupWhenHeader endpoints
cvps1d.Length
cvps1d[7]
let cvps1e =
    cvps1d
    |> List.map (constructSubgraph ontoGraph1)
cvps1e.Length
cvps1e.Head
cvps1e[5]
let cvps1f =
    cvps1e
    |> List.mapi (fun i e -> printfn "%i" i; completeOpenEnds ontoGraph1 e)
let res1a = ArcGraph.


let res2 = fromXlsxFile (ontologyToFGraph Terms.AssayMetadata.ontology) Assay.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\assays\measurement1\isa.assay.xlsx"

res2 |> Seq.iter (Visualization.isaGraphToFullCyGraph >> CyGraph.show)

let getSubsequentFollowsTerm onto cvp =
    getPrecedingCvParams cvp onto
    |> Seq.pick (fun (id,t,r) -> if r.HasFlag ArcRelation.Follows then Some t else None)



Seq.zip doneGraphComplicated.Keys doneGraphComplicated.Values
|> Seq.map (
    fun (nk1,c) -> 
        if FContext.predecessors c |> Seq.isEmpty then 
            printfn "empty preds @ %A" nk1
            getSubsequentFollowsTerm ontoGraph (c |> fun (p,nd,s) -> nd)
            |> fun r -> printfn "term: %A" r; r
        else OboTerm.Create ""
)
|> List.ofSeq
|> ignore



completeOpenEnds ontoGraph doneGraphComplicated |> isaGraphToFullCyGraph |> CyGraph.show
let wrongTermInContacts = ArcGraph.fromXlsxFile ontoGraph Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation_wrongTermInContacts.xlsx"
wrongTermInContacts |> List.ofSeq
let wrongTermInContactsF = Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation_wrongTermInContacts.xlsx"

let res0 = fromXlsxFile ontoGraph Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation.xlsx"
res0 |> Seq.head |> Visualization.isaGraphToFullCyGraph |> CyGraph.show
res0 |> Seq.item 1 |> Visualization.isaGraphToFullCyGraph |> CyGraph.withLayout (Layout.initGrid (Layout.LayoutOptions.Cose(NodeRepulsion = 500000000))) |> CyGraph.show
res0 |> Seq.iter (Visualization.isaGraphToFullCyGraph >> CyGraph.show)
res0 |> Seq.toList
res0
|> Seq.iteri (
    fun i e ->
        printfn "%i" i
        Visualization.isaGraphToFullCyGraph e
        |> ignore
)

let eps = getPartOfEndpoints ontoGraph
cvparamse 
|> deletePartOfEndpointSectionKeys eps
|> groupWhenHeader eps
|> List.map (constructSubgraph ontoGraph)
|> List.mapi (
    fun i x -> completeOpenEnds ontoGraph x
)


let res1 = fromXlsxFile (ontologyToFGraph Terms.StudyMetadata.ontology) Study.parseMetadataSheetfromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\studies\experiment1_material\isa.study.xlsx"
let res1 = fromXlsxFile (ontologyToFGraph Terms.StudyMetadata.ontology) Study.parseMetadataSheetfromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\studies\experiment1_material\isa.study_unnecessarilyFilled.xlsx"

res1 |> Seq.iter (Visualization.isaGraphToFullCyGraph >> CyGraph.show)
res1 |> Seq.last |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.toList
res1 |> Seq.head |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.item 6 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.item 7 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.item 8 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.iteri (fun i _ -> printfn "%i" i)
res1 |> Seq.item 100 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.head


let ontoGraph1 = ontologyToFGraph Terms.StudyMetadata.ontology
let endpoints = getPartOfEndpoints ontoGraph1
let cvps1a = Study.parseMetadataSheetfromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\studies\experiment1_material\isa.study_unnecessarilyFilled.xlsx" 
cvps1a.Length
let cvps1b = cvps1a |> List.choose (Param.tryCvParam)
cvps1b.Length
let cvps1c = cvps1b |> deletePartOfEndpointSectionKeys endpoints
cvps1c.Length
cvps1c |> List.iter (fun x -> printfn "%s" x.Name)
let cvps1d = cvps1c |> groupWhenHeader endpoints
cvps1d.Length
cvps1d[7]
let cvps1e =
    cvps1d
    |> List.map (constructSubgraph ontoGraph1)
cvps1e.Length
cvps1e.Head
cvps1e[5]
let cvps1f =
    cvps1e
    |> List.mapi (fun i e -> printfn "%i" i; completeOpenEnds ontoGraph1 e)
let res1a = ArcGraph.


let res2 = fromXlsxFile (ontologyToFGraph Terms.AssayMetadata.ontology) Assay.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\assays\measurement1\isa.assay.xlsx"

res2 |> Seq.iter (Visualization.isaGraphToFullCyGraph >> CyGraph.show)

/// Takes an ISA-based ontology FGraph and a list of CvParams and returns the CvParams grouped into lists of sections.
let groupWhenHeader onto (cvps : CvParam list) =
    let endpoints = getPartOfEndpoints onto
    cvps
    |> List.groupWhen (isHeader endpoints)

groupWhenHeader ontoGraph cvparamse
|> List.map (List.map (fun c -> c.Name))


/// Takes an ISA-based ontology FGraph, an XLSX parsing function and a path to an XLSX file and returns a seq of section-based ISA-structured subgraphs.
/// 
/// `xlsxParsing` can be any of `Investigation.parseMetadataSheetFromFile`, `Study.parseMetadataSheetFromFile`, or `Assay.parseMetadataSheetFromFile`.
let fromXlsxFile onto (xlsxParsing : string -> IParam list) xlsxPath =
    let cvps = xlsxParsing xlsxPath |> List.choose (Param.tryCvParam)
    let groupedCvps = groupWhenHeader onto cvps
    groupedCvps
    |> Seq.map (
        ArcGraph.constructSubgraph onto 
        >> completeOpenEnds onto
    )

let res0 = fromXlsxFile ontoGraph Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation.xlsx"
res0 |> Seq.head |> Visualization.isaGraphToFullCyGraph |> CyGraph.show
res0 |> Seq.item 1 |> Visualization.isaGraphToFullCyGraph |> CyGraph.withLayout (Layout.initGrid (Layout.LayoutOptions.Cose(NodeRepulsion = 500000000))) |> CyGraph.show
res0 |> Seq.iter (Visualization.isaGraphToFullCyGraph >> CyGraph.show)
res0 |> Seq.toList
res0
|> Seq.iteri (
    fun i e ->
        printfn "%i" i
        Visualization.isaGraphToFullCyGraph e
        |> ignore
)

let eps = getPartOfEndpoints ontoGraph
cvparamse 
|> deletePartOfEndpointSectionKeys eps
|> groupWhenHeader eps
|> List.map (constructSubgraph ontoGraph)
|> List.mapi (
    fun i x -> completeOpenEnds ontoGraph x
)

let res1 = fromXlsxFile (ontologyToFGraph Terms.StudyMetadata.ontology) Study.parseMetadataSheetfromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\studies\experiment1_material\isa.study.xlsx"
let res1 = fromXlsxFile (ontologyToFGraph Terms.StudyMetadata.ontology) Study.parseMetadataSheetfromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\studies\experiment1_material\isa.study_unnecessarilyFilled.xlsx"

res1 |> Seq.iter (Visualization.isaGraphToFullCyGraph >> CyGraph.show)
res1 |> Seq.last |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.toList
res1 |> Seq.head |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.item 6 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.item 7 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.item 8 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.iteri (fun i _ -> printfn "%i" i)
res1 |> Seq.item 100 |> Visualization.printGraph (fun nd -> nd.Name)
res1 |> Seq.head


let ontoGraph1 = ontologyToFGraph Terms.StudyMetadata.ontology
let endpoints = getPartOfEndpoints ontoGraph1
let cvps1a = Study.parseMetadataSheetfromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\studies\experiment1_material\isa.study_unnecessarilyFilled.xlsx" 
cvps1a.Length
let cvps1b = cvps1a |> List.choose (Param.tryCvParam)
cvps1b.Length
let cvps1c = cvps1b |> deletePartOfEndpointSectionKeys endpoints
cvps1c.Length
cvps1c |> List.iter (fun x -> printfn "%s" x.Name)
let cvps1d = cvps1c |> groupWhenHeader endpoints
cvps1d.Length
cvps1d[7]
let cvps1e =
    cvps1d
    |> List.map (constructSubgraph ontoGraph1)
cvps1e.Length
cvps1e.Head
cvps1e[5]
let cvps1f =
    cvps1e
    |> List.mapi (fun i e -> printfn "%i" i; completeOpenEnds ontoGraph1 e)
let res1a = ArcGraph.


let res2 = fromXlsxFile (ontologyToFGraph Terms.AssayMetadata.ontology) Assay.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\assays\measurement1\isa.assay.xlsx"

res2 |> Seq.iter (Visualization.isaGraphToFullCyGraph >> CyGraph.show)

let getSubsequentFollowsTerm onto cvp =
    getPrecedingCvParams cvp onto
    |> Seq.pick (fun (id,t,r) -> if r.HasFlag ArcRelation.Follows then Some t else None)



Seq.zip doneGraphComplicated.Keys doneGraphComplicated.Values
|> Seq.map (
    fun (nk1,c) -> 
        if FContext.predecessors c |> Seq.isEmpty then 
            printfn "empty preds @ %A" nk1
            getSubsequentFollowsTerm ontoGraph (c |> fun (p,nd,s) -> nd)
            |> fun r -> printfn "term: %A" r; r
        else OboTerm.Create ""
)
|> List.ofSeq
|> ignore



completeOpenEnds ontoGraph doneGraphComplicated |> isaGraphToFullCyGraph |> CyGraph.show

//let constructSubraph iOuter isaOntoGraph (cvParams : CvParam list) =
//    let rec loop i (inputList : CvParam list) (collList : CvParam list) (priorHead : CvParam) sectionHeader (graph : FGraph<int*string,CvParam,string>) =
//        match inputList with
//        | [] -> graph
//        | h :: t ->
//            if hasFollowsTo isaOntoGraph h priorHead then
//                if hasPartOfTo isaOntoGraph h priorHead then
//                    FGraph.addElement (i + 1,h.Accession) h (i,priorHead.Accession) priorHead "" graph
//                    |> loop (i + 1) t collList h (i,priorHead.Accession,priorHead)
//                elif equalsPartOf isaOntoGraph h priorHead then
//                    FGraph.addElement (i + 1,h.Accession) h (i,priorHead.Accession) priorHead "" graph
//                    |> loop (i + 1) t collList h sectionHeader
//                else
//                    FGraph.addElement (i + 1,collList.Head.Accession) collList.Head (sectionHeader |> fun (i,id,t) -> i,id) (sectionHeader |> fun (i,id,t) -> t) "" graph
//                    |> loop (i + 1) collList.Tail t collList.Head sectionHeader
//            elif equalsPartOf isaOntoGraph h priorHead then
//                if CvParam.equalsTerm (CvParam.getTerm h) priorHead then
//                    loop i 
//    loop iOuter cvParams.Tail [] cvParams.Head 





// problem: approach too big, try working only with sections
///// Takes an ontology graph and a list of CvParams and constructs an ISA graph from them.
//let constructIsaGraph isaOntoGraph (cvParams : CvParam list) =
//    let rec loop i inputList collList priorHead priorSectionHeader (graph : FGraph<int*string,CvParam,string>) =
//        match inputList with
//        | h :: t ->
//            match hasFollowsTo isaOntoGraph h priorHead with
//            | true ->
//                match hasPartOfTo isaOntoGraph h priorHead with
//                | true ->
//                    FGraph.addElement (i + 1,h.Accession) h (i,priorHead.Accession) priorHead "" graph
//                    |> loop (i + 1) t collList h (i,priorHead.Accession)
//                | false ->
//                    match equalsPartOf isaOntoGraph h priorHead with
//                    | true ->
//                        FGraph.addElement (i + 1,h.Accession) h (i,priorHead.Accession) priorHead "" graph
//                        |> loop (i + 1) t collList h priorSectionHeader
//                    | false ->
//                        // go through collList and add to graph
//                        loop i collList t 
//            | false ->
//                match hasPartOfTo isaOntoGraph h priorHead with
//                | true ->
//                    failwith $"{h.Accession} has no follows but part_of to prior head {priorHead.Accession}. This should never happen since Column 1 must be correct."
//                | false ->
//                    match equalsPartOf isaOntoGraph h priorHead with
//                    | true ->
//                        match CvParam.equalsTerm (CvParam.getTerm h) priorHead with
//                        | true ->
//                            loop i t (h :: collList) priorHead priorSectionHeader graph
//                        | false ->
//                            // get previous elements, init them empty and add them to graph
//                    | false ->
//                        failwith $"{h.Accession} has no follows but shares the same part_of as prior head {priorHead.Accession}. This should never happen since Column 1 must be correct"
//        | [] -> graph
//    loop 0 cvParams.Tail [] cvParams.Head (0,"") FGraph.empty<int*string,CvParam,string>

///// Takes an ontology graph and a list of CvParams and constructs an ISA graph from them.
//let constructIsaGraph isaOntoGraph (cvParams : CvParam list) =
//    let rec loop i inputList collList priorHead (graph : FGraph<int*string,CvParam,ArcRelation>) =
//        match inputList with
//        | h :: t ->
//            match getFollows isaOntoGraph h priorHead, getPartOf isaOntoGraph h priorHead with
//            | Some (id1,t1,r1), Some (id2,t2,r2) -> 
//                FGraph.addElement (i + 1, h.Accession) h (i, priorHead.Accession) priorHead r1 graph
//                |> loop (i + 1) t collList h 
//            | None, Some (id,t,r) -> failwith $"{h.Accession} has no follows but part_of to prior head. This should never happen since Column 1 must be correct."
//            | Some (id,t,r), None ->
//            | None, None -> loop i t (priorHead :: collList) h graph
//        | [] -> graph
//    loop 0 cvParams.Tail [] cvParams.Head FGraph.empty<int*string,CvParam,ArcRelation>

// OOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOO


let getRelatedTermByRelation relation term onto =
    OboOntology.getRelatedTerms term onto 
    |> List.choose (
        fun (term1,relationship,term2) -> 
            if toRelation relationship = relation then
                term2
            else None
    )

let getOboTerm onto (cvp : CvParam) =
    onto.Terms |> List.find (fun term -> term.Id = cvp.Accession)

let tryGetOboTerm onto (cvp : CvParam) =
    onto.Terms |> List.tryFind (fun term -> term.Id = cvp.Accession)

let toNodeCyGraph (fGraph : FGraph<_,_,_>) =
    CyGraph.initEmpty ()
    |> CyGraph.withElements (
            let nks = fGraph.Keys |> List.ofSeq
            let nls = fGraph.Values |> Seq.toList |> List.map (fun (i,s,o) -> s.ToString())
            List.zip nks nls
            |> List.map (fun (nk,nl) -> Elements.node (string nk) [CyParam.label nl])
        )
    |> CyGraph.withStyle "node"     
        [
            CyParam.content =. CyParam.label
            CyParam.color "#A00975"
        ]

toNodeCyGraph res |> CyGraph.show

let toFullCyGraph (fGraph : FGraph<string,OboTerm,ArcRelation>) =
    CyGraph.initEmpty ()
    |> CyGraph.withElements [
            for (nk1,nd1,nk2,nd2,e) in FGraph.toSeq fGraph do
                let nk1s = sprintf "%s" nk1
                let nk2s = sprintf "%s" nk2
                Elements.node nk1s [CyParam.label nd1.Name]
                Elements.node nk2s [CyParam.label nd2.Name]
                Elements.edge (sprintf "%s_%s" nk1s nk2s) nk1s nk2s [
                    CyParam.label <| e.ToString()
                    match e with
                    | ArcRelation.Follows -> CyParam.color "red"
                    | ArcRelation.PartOf -> CyParam.color "blue"
                    | x when x = ArcRelation.PartOf + ArcRelation.Follows -> CyParam.color "purple"
                ]
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
    |> CyGraph.withSize(1920, 1080)

toFullCyGraph res |> CyGraph.show

res |> FGraph.toSeq |> Seq.map (fun )


// building the follows graph

let constructFollowsGraph onto cvps =
    let rec loop inputList (collList : CvParam list) failedList i graph =
        match inputList with
        | h1 :: t1 ->
            match collList with
            | [] -> loop t1 (h1 :: collList) failedList i graph
            | h2 :: t2 ->
                let cvpTerm = onto.Terms |> List.find (fun term -> term.Id = h1.Accession)
                let relatedTerms = getRelatedTermByRelation Relation.Follows cvpTerm onto
                if relatedTerms.Length = 0 then
                    //loop inputList t2 (h2 :: failedList) i graph      // maybe in some cases correct? investigate!
                    loop inputList t2 failedList i graph
                elif relatedTerms |> List.tryFind (fun rt -> rt.Id = h2.Accession) |> Option.isSome then
                    FGraph.addElement (i + 1,h1.Name) h1 (i,h2.Name) h2 Relation.Follows graph
                    |> loop inputList t2 failedList (i + 1)
                else
                    loop inputList t2 (h2 :: failedList) i graph
        | [] -> graph, List.rev failedList
    loop cvps [] [] 0 FGraph.empty<int*string,CvParam,Relation>

let resGraph, faileds = constructFollowsGraph obo (cvparamse |> List.filter (fun cvp -> cvp.GetAttribute("Column") |> Param.getValue = 1))
//toFullCyGraph resGraph |> CyGraph.show


for (nk1,nd1,nk2,nd2,e) in FGraph.toSeq resGraph do
    let nk1s = sprintf "%i, %s" (fst nk1) (snd nk1)
    let nk2s = sprintf "%i, %s" (fst nk2) (snd nk2)
    printfn "Rows%i->%i   %s ---%A---> %s" (nd1.GetAttribute "Row" |> Param.getValueAsInt) (nd2.GetAttribute "Row" |> Param.getValueAsInt) nk1s e nk2s

faileds |> List.iter (fun cvp -> printfn "Row%i   %s" (cvp.GetAttribute "Row" |> Param.getValueAsInt) cvp.Name)


// building the part_of graph

let paramsCtcs = Investigation.parseMetadataSheetFromFile @"C:\Repos\git.nfdi4plants.org\ArcPrototype\isa.investigation_ContactsOnly.xlsx"

let cvpsCtcs = paramsCtcs |> List.map (Param.tryCvParam >> Option.get)

// look for what is next follows term. if 
// - it matches + has same part_of: connect it to previous
// - it does not match but is same follows + same part_of: put to collList
// - it does not match + has not same part_of: 

let constructPartOfGraph onto cvps =
    let rec loop inputList (collList : CvParam list) i graph =
        match inputList with
        | h1 :: t1 ->
            match collList with
            | [] -> loop t1 (h1 :: collList) i graph
    loop cvps [] 0 FGraph.empty<int*string,CvParam,Relation>




//type IOntologyEntry =
//    abstract member id : string
//    abstract member name : string


module Error =

    module MissingEntity =

        type MissingValue =
            //interface IOntologyEntry with
                static member id = "DPEO:00000003"
                static member name = "Missing Value"

        type MissingMetadataKey =
                static member id = "DPEO:00000004"
                static member name = "Missing Metadata Key"


module InvestigationMetadata =

    module InvestigationContacts =

        type InvestigationPersonLastName =
            //interface IOntologyEntry with
                static member id = "INVMSO:00000022"
                static member name = "Investigation Person Last Name"

        type InvestigationPersonFirstName =
            //interface IOntologyEntry with
                static member id = "INVMSO:00000023"
                static member name = "Investigation Person First Name"

type InvestigationMetadata =
    static member id = "INVMSO:00000001"
    static member name = "Investigation Metadata"

let metadataSections = [
    21, InvestigationMetadata.InvestigationContacts.InvestigationPersonLastName.name
    22, InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name
]



type ARCValidateContext = {
        Tokens          : Dictionary<string,IParam list>
        TestConditions  : Dictionary<string,bool>
        Filepath        : string
    }

    with
        static member create tokens testConditions filepath = {
            Tokens          = tokens
            TestConditions  = testConditions
            Filepath        = filepath
        }

        static member getTokens (tokenName : string) (arcValidateContext : ARCValidateContext) =
            arcValidateContext.Tokens[tokenName]

        static member tryGetTokens (tokenName : string) (arcValidateContext : ARCValidateContext) =
            Dictionary.tryFind tokenName arcValidateContext.Tokens

        static member getTestCondition (testName : string)  (arcValidateContext : ARCValidateContext) =
            arcValidateContext.TestConditions[testName]

        static member tryGetTestCondition (testName : string) (arcValidateContext : ARCValidateContext) =
            Dictionary.tryFind testName arcValidateContext.TestConditions

        static member addTestCondition (testName : string) (testResult : bool) (arcValidateContext : ARCValidateContext) =
            arcValidateContext.TestConditions.Add(testName, testResult)

let dependentTestCase testName (conditionDic : Dictionary<string,bool>) dicKey testCase =
    test testName {
        if conditionDic[dicKey] then
            testCase ()
        else
            skiptestf $"Skipped due to failed test: {dicKey}."
    }

//let testListExpanded name tests =
//    testList name 

let testCaseArc (arcValidateContext : ARCValidateContext) (error : string) (position : string) (test : ARCValidateContext -> string -> string -> unit) =
    let name = $"{error} test: {position}"
    testCase name (fun () -> test arcValidateContext name position)

let testCaseArcDependent (arcValidateContext : ARCValidateContext) (error : string) (position : string) dependsOnTest (test : ARCValidateContext -> string -> string -> unit) =
    let name = $"{error} test: {position}"
    dependentTestCase name arcValidateContext.TestConditions dependsOnTest (fun () -> test arcValidateContext name position)


type ARCTest = {
        Name            : string
        Test            : Test
        //Dependencies    : ARCTest list
    }

    with
        static member Create(error : string, position : string, test : Test (*dependencies*)) = {
            Name            = $"{error} test: {position}"
            Test            = test
            //Dependencies    = dependencies
        }

        static member Create(error : string, position : string, arcValidateContext : ARCValidateContext, test : ARCValidateContext -> string -> string -> unit) = {
            Name            = $"{error} test: {position}"
            Test            = testCaseArc arcValidateContext error position test
        }

        static member CreateDependent(error : string, position : string, arcValidateContext : ARCValidateContext, test : ARCValidateContext -> string -> string -> unit, dependsOnTest) = {
            Name            = $"{error} test: {position}"
            Test            = testCaseArcDependent arcValidateContext error position dependsOnTest test
        }




let createErrorStack path =
    $"'{path}'"

let createErrorStackWithLinePos path line position =
    $"'{path}' > line '{line}' > position '{position}'"

let createErrorStackWithCell path sheet row column =
    let cellString = FsSpreadsheet.FsAddress(row, column).Address
    $"'{path}' > sheet '{sheet}' > cell '{cellString}'"

let createErrorStackWithColumn path sheet column =
    $"'{path}' > sheet '{sheet}' > column '{column}'"

let createErrorStackWithRow path sheet row =
    $"'{path}' > sheet '{sheet}' > row '{row}'"

//let getRelativePath fullpath =      // alternative names: `getRelativeArcPath`, `getArcRelativePath`, `get
    


let invPath = @"C:/Repos/gitlab.nfdi4plants.org/ArcPrototype/isa.investigation.xlsx"

let inv : IParam list = try ARCTokenization.Investigation.parseMetadataSheetFromFile invPath with _ -> []
//let inv : IParam list = []

// gucken wer das in Git Blame verbrochen hat!
let invDict = Dictionary.ofList (inv |> List.groupBy CvBase.getCvName) :?> Dictionary<string,IParam list>

let getMetadataSectionKey iParamList = 
    iParamList
    |> List.filter (fun ip -> Param.getValueAsString ip = (Terms.StructuralTerms.metadataSectionKey |> CvTerm.getName))
    |> List.exactlyOne

(invDict["Study Person First Name"].Head :?> CvParam).GetAttribute(Address.row) |> Param.getValueAsInt
(invDict["Study Person First Name"][1] :?> CvParam).GetAttribute(Address.row) |> Param.getValueAsInt










module Expect =

    module ARC =

        /// Checks if a given value can by matched by a given Regex pattern. Else fails and returns given error message.
        let matchPattern (regex : Regex) value errorMessage =
            let regexRes = regex.Match value
            if not regexRes.Success then
                failtestf "%s" errorMessage


        module Dict =

            /// Checks if a given Dictionary contains a given key.
            let contains key dict errorMessage =
                if not (Dictionary.containsKey key dict) then
                    failtestf "%s" errorMessage

            /// Checks if a given Dictionary has a value under a given key. Else fails and returns given error message.
            /// 
            /// This differentiates by `contains` via assuming the key is present but evaluating if the value is Some or None.
            let hasValue key dict errorMessage =
                if (Dictionary.tryFind key dict).IsNone then
                    failtestf "%s" errorMessage

            /// Checks if a given Dictionary under a given key has a value that satisfies the predicate. Else fails and returns given error message.
            let hasValueBy predicate key dict errorMessage =
                match Dictionary.tryFind key dict with
                | Some value ->
                    if not (predicate value) then failtestf "%s" errorMessage
                | None -> failtestf "%s" errorMessage

            /// Checks if a given Dictionary under a given key has a value that equals the given value. Else fails and returns given error message.
            let equalsValue key value dict errorMessage =
                if not (Dictionary.item key dict = value) then
                    failtestf "%s" errorMessage

            /// Checks if a given Dictionary under a given key has a value that equals the given value after applying a given mapping function. Else fails and returns given error message.
            let equalsValueBy mapping key value dict errorMessage =
                if not (Dictionary.item key dict |> mapping = value) then
                    failtestf "%s" errorMessage


        module Graph =

            /// Checks if a given key (i.e. NodeKey) exists in a given FGraph. Else fails and returns given error message.
            let exists key graph errorMessage =
                if not (FGraph.containsNode key graph) then
                    failtestf "%s" errorMessage

            /// Checks if a given key (i.e. NodeKey) has neighbours at a given level in a given FGraph. Else fails and returns given error message.
            let hasNeighbours key level (graph : FGraph<_,_,_>) errorMessage =
                let rec loop lvl currentNeighbours =
                    if level = lvl then 
                        currentNeighbours
                        |> Seq.exists (
                                fun t -> graph.Item t
                                >> FContext.neighbours 
                                >> Seq.isEmpty 
                                >> not
                        )
                    else 
                        let furtherNeighbours =
                            currentNeighbours
                            |> Seq.fold (
                                fun acc n -> 
                                    FContext.neighbours (graph.Item n)
                            )
                        loop (lvl + 1)
                // CARE! `FContext.neighbours` rates Nodes that have circular Edges (Edges that point to themselve) as neighbours. Exclude them if this becomes a problem in future applications...
                let res = FContext.neighbours (graph.Item key)
                raise <| System.NotImplementedException()

            /// Checks if a given key (i.e. NodeKey) has neighbours that satisfy a given predicate at a given level in a given FGraph. Else fails and returns given error message.
            let hasNeighboursBy key level predicate graph errorMessage =
                raise <| System.NotImplementedException()

            //let extract key subtree graph errorMessage =
            //    raise <| System.NotImplementedException()

            //let tryGet key graph errorMessage =     // ist das nicht dasselbe wie exists?
            //    raise <| System.NotImplementedException()

            //let tryGetBy mapping key graph errorMessage =     // ist das nicht dasselbe wie exists?
            //    raise <| System.NotImplementedException()




// !!!!!!!!!!SEHR GUT!!!!!!!!!!!!
module ArcExpect =
// alternativ: Expect.ARC.isNonEmpty usw.

    // davon werden wir vllt. 10 St�ck oder so brauchen
    let hasMetadataSectionKey (arcValidateContext : ARCValidateContext) testName key =
        match Dictionary.tryFind key arcValidateContext.Tokens with
        | Some value -> 
            try 
                getMetadataSectionKey value |> ignore
                ARCValidateContext.addTestCondition testName true arcValidateContext
            with
                | _ -> 
                    ARCValidateContext.addTestCondition testName false arcValidateContext
                    failtestf "%s" (createErrorStack arcValidateContext.Filepath)
        | None -> 
            ARCValidateContext.addTestCondition testName false arcValidateContext
            failtestf "%s" (createErrorStack arcValidateContext.Filepath)

    /// 
    let hasValues (arcValidateContext : ARCValidateContext) testName key =
        match Dictionary.tryFind key arcValidateContext.Tokens with
        | Some value -> 
            let mdsk = getMetadataSectionKey value
            let row = (mdsk :?> CvParam).GetAttribute(Address.row) |> Param.getValueAsInt
            let col = ((mdsk :?> CvParam).GetAttribute(Address.column) |> Param.getValueAsInt) + 1
            let sheet = (mdsk :?> CvParam).GetAttribute(Address.worksheet) |> Param.getValueAsString
            //let message = Message.Create(invPath, XLSXFileKind, row, col, sheet)
            value       // hier muss das filtern noch raus, das soll bereits vorher passieren
            |> List.filter (fun ip -> Param.getValueAsString ip <> (Terms.StructuralTerms.metadataSectionKey |> CvTerm.getName))
            |> fun res ->
                match res with
                | [] ->
                    ARCValidateContext.addTestCondition testName false arcValidateContext
                    failtestf "%s" (createErrorStackWithCell invPath sheet row col)
                | _ -> ARCValidateContext.addTestCondition testName true arcValidateContext
        | None -> 
            ARCValidateContext.addTestCondition testName false arcValidateContext
            failtestf "%s" (createErrorStack arcValidateContext.Filepath)

    let hasAllMetadataSectionKeys (arcValidateContext : ARCValidateContext) testName keyList =
        keyList
        |> List.iter (hasMetadataSectionKey arcValidateContext testName)



let tl = 
    testSequenced (
        testList "Critical" [
            let myArcContext = ARCValidateContext.create invDict (Dictionary()) invPath
            let areAllMetadataSectionKeysPresentTest =
                ARCTest.Create(
                    error = Error.MissingEntity.MissingMetadataKey.name,
                    position = InvestigationMetadata.name,
                    arcValidateContext = myArcContext,
                    test = ArcExpect.hasMetadataSectionKey
                )
            let hasMetadataSectionKeyTest = 
                ARCTest.Create(
                    error = Error.MissingEntity.MissingMetadataKey.name,
                    position = InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name,
                    arcValidateContext = myArcContext,
                    test = ArcExpect.hasMetadataSectionKey
                )
            let hasValuesTest =
                ARCTest.CreateDependent(
                    error = Error.MissingEntity.MissingValue.name,
                    position = InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name,
                    arcValidateContext = myArcContext,
                    dependsOnTest = hasMetadataSectionKeyTest.Name,
                    test = ArcExpect.hasValues
                )
            areAllMetadataSectionKeysPresentTest.Test
            hasMetadataSectionKeyTest.Test
            hasValuesTest.Test
        ]
    )

tl |> performTest

    //testCase $"{Error.MissingEntity.MissingValue.name} test: {InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name}" <| fun () -> 
    //    ArcExpect.isNotEmpty invDict "Investigation Person First Name"













    //let exists

Error.MissingEntity.MissingValue.name

testList "Critical" [
    //testCase 
    testCaseArc 
        Error.MissingEntity.MissingValue.name 
        InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name 

        ArcExpect.isNotEmpty
        //ArcExpect.isNotEmpty invDict "Investigation Person First Name"
]
|> performTest
// !!!!!!!!!!!!!!!!!!!!!!!!!

ArcExpect.isNonEmpty invDict "Investigation Person First Name"


// ValidationResult nicht notwendig, stattdessen alles in Expecto-Funktion evaluieren
// MetadataSection aus dem Dictionary raus
// Addresse bekommen in eine schmale Funktion / zu einer Funktion machen
// createErrorStack-Funktion soll CvParam, Pfad und ErrorOntologyTermName als Parameter bekommen und daraus Fehlermeldung zur�ckgeben
// "wrong format" fehlt noch in der ErrorOntology
// Expectos Expect.blabla Funktionen alle f�r uns so schreiben, dass es von der Message her passt

let hasPersonFirstNames = 
    if Dictionary.containsKey "Investigation Person First Name" invDict then
        invDict["Investigation Person First Name"]
        |> fun ipl -> 
            let values =
                ipl
                |> List.filter (fun ip -> Param.getValueAsString ip <> (Terms.StructuralTerms.metadataSectionKey |> CvTerm.getName))
            let check = List.isEmpty values |> not
            if check then
                Success
            else
                let mdsk = getMetadataSectionKey ipl
                let row = (mdsk :?> CvParam).GetAttribute(Address.row) |> Param.getValueAsInt
                let col = ((mdsk :?> CvParam).GetAttribute(Address.column) |> Param.getValueAsInt) + 1
                let sheet = (mdsk :?> CvParam).GetAttribute(Address.worksheet) |> Param.getValueAsString
                let message = Message.Create(invPath, XLSXFileKind, row, col, sheet)
                Error message
    else Error (Message.Create(invPath, XLSXFileKind, 0, 0, ""))
    |> fun res -> 
        testCase InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name (fun _ ->
            res
            |> throwError (
                fun m -> 
                    createErrorStackXlsxFile 
                        m 
                        InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name 
                        Error.MissingEntity.MissingValue.name
            )
        )



let case = 
    testCase InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name (fun _ ->
        hasPersonFirstNames
        |> throwError (
            fun m -> 
                createErrorStackXlsxFile 
                    m 
                    InvestigationMetadata.InvestigationContacts.InvestigationPersonFirstName.name 
                    Error.MissingEntity.MissingValue.name
        )
    )




case |> performTest

//let ups = inv |> List.choose UserParam.tryUserParam
//let cvpsEmptyVals = inv |> List.choose CvParam.tryCvParam |> List.filter (Param.getValueAsString >> (=) "")

//let inv = ARCTokenization.Investigation.parseMetadataRowsFromFile @"C:/Repos/gitlab.nfdi4plants.org/ArcPrototype/isa.investigation.xlsx"

//inv[20]

//Param.getValueAsTerm (CvParam("1", "2", "3", ParamValue.Value ""))
//Param.getValueAsString (CvParam("1", "2", "3", ParamValue.Value ""))
//Param.getValueAsString (CvParam("1", "2", "3", ParamValue.CvValue ("1", "Pimmel", "3")))
//Param.getValueAsTerm (CvParam("1", "2", "3", ParamValue.CvValue ("1", "Pimmel", "3")))