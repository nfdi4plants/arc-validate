//#I "../../fslaborg/Graphoscope/src/Graphoscope/bin/Debug/netstandard2.0"
//#I "../../omaus/Graphoscope/src/Graphoscope/bin/Debug/netstandard2.0"
//#r "Graphoscope.dll"
#r "nuget: OBO.NET"
#r "nuget: FSharpAux"
#r "nuget: Graphoscope"

open OBO.NET
open FSharpAux
open Graphoscope

open System.Text.RegularExpressions
open System.IO


let eco = OboOntology.fromFile true (Path.Combine(__SOURCE_DIRECTORY__, "ErrorClassOntology.obo"))

eco.Terms.Head
let no5 = eco.Terms[5]

no5.Id

let testTerms = [
    OboTerm.Create("test:00000000", Name = "test")
    OboTerm.Create("test:00000001", Name = "test2a", IsA = ["test:00000000"])
    OboTerm.Create("test:00000002", Name = "test3", IsA = ["test:00000001"])
    OboTerm.Create("test:00000003", Name = "test2b", IsA = ["test:00000000"])
]

//let testOntology = OboOntology.create testTerms []
let testOntology = OboOntology.fromFile true (Path.Combine(__SOURCE_DIRECTORY__, "TestOntology.obo"))

//OboOntology.toFile (Path.Combine(__SOURCE_DIRECTORY__, "TestOntology.obo")) testOntology

let templateModuleString = """module ``<name>`` =
    let key = CvTerm.create(<id>, <name>, <ref>)
"""

let templateTypeString = """type <name> =
    static member name = "<name>"
    static member id = "<id>"
"""

String.replicate 2 "    "


let indent level input =
    let spaces = String.replicate level "    "
    input
    |> String.toLines
    |> Seq.map (fun s -> $"{spaces}{s}")
    |> Seq.reduce (fun x y -> $"{x}\n{y}")

indent 1 templateModuleString
|> printfn "%s"


/// Takes a relationship as input and returns its name and ID as anonymous record if they exist. Else returns None.
let tryDeconstructRelationship relationship =
    // matches patterns in the format of "<text> <text>:<numbers> ! <text with spaces>", e.g. "part_of INVMSO:00000001 ! Investigation Metadata"
    let pattern = Regex @"^(?<relName>\w+)\s(?<id>\w+:\d+)\s!\s.*$"
    let matchResult = pattern.Match relationship
    let relName = matchResult.Groups["relName"].Value
    let id = matchResult.Groups["id"].Value
    if String.isNullOrEmpty relName || String.isNullOrEmpty id then None
    else 
        Some {|
            RelationshipName    = relName
            Id                  = id
        |}

/// Takes a relationship as input and returns its name and ID as anonymous record.
let deconstructRelationship relationship =
    // matches patterns in the format of "<text> <text>:<numbers> ! <text with spaces>", e.g. "part_of INVMSO:00000001 ! Investigation Metadata"
    let pattern = Regex @"^(?<relName>\w+)\s(?<id>\w+:\d+)\s!\s.*$"
    let matchResult = pattern.Match relationship
    let relName = matchResult.Groups["relName"].Value
    let id = matchResult.Groups["id"].Value
    {|
        RelationshipName    = relName
        Id                  = id
    |}

/// <summary>
/// Adds a given term as a node to a graph.
/// </summary>
/// <param name="term">The term.</param>
/// <param name="graph">The graph that the term is added to.</param>
let addTerm (graph : FGraph<string,OboTerm,string>) (term : OboTerm) =
    FGraph.Node.add term.Id term graph

let addRelation (graph : FGraph<string,OboTerm,string>) (term : OboTerm) =()

/// <summary>
/// Adds the relationship of a given term as an edge to a graph.
/// 
/// Currently, only `part_of` and `has_a` relationships are supported.
/// </summary>
/// <param name="relationship">The relationship.</param>
/// <param name="term">The relationship's term.</param>
/// <param name="graph">The graph which the relationship is added to.</param>
let addRelationship (graph : FGraph<string,OboTerm,string>) (term : OboTerm) relationship =
    let decRel = tryDeconstructRelationship relationship
    match decRel with
    | Some dr -> 
        match dr.RelationshipName.ToLower() with
        | "part_of" -> FGraph.Edge.add term.Id dr.Id dr.RelationshipName graph
        | "has_a" -> FGraph.Edge.add dr.Id term.Id dr.RelationshipName graph
        | _ -> graph
    | None -> graph

/// <summary>
/// Adds the is_a relation of a given term as an edge to a graph.
/// </summary>
/// <param name="isA">The is_a relation.</param>
/// <param name="term">The is_a's term.</param>
/// <param name="graph">The graph which the is_a relation is added to.</param>
let addIsA (graph : FGraph<string,OboTerm,string>) (term : OboTerm) isA =
    FGraph.Edge.add term.Id isA "is_a" graph

/// <summary>
/// Adds the synonym of a given term as an edge to a graph if the synonym's term exists in the ontology.
/// </summary>
/// <param name="synoynm">The synonym.</param>
/// <param name="term">The term that has the synonym.</param>
/// <param name="ontology">The ontology of the term.</param>
/// <param name="graph">The graph that has the is_a relation.</param>
let tryAddSynonym (graph : FGraph<string,OboTerm,string>) (term : OboTerm) (ontology : OboOntology) synonym =
    // CARE: `.Text` is not Term ID but name
    let revisedSynonymText = String.replace "\"" "" synonym.Text    // because sometimes the synonym text has additional quotation marks that need to be eradicated
    let synonymTerm =
        ontology.Terms
        |> List.tryFind (fun t -> t.Name = revisedSynonymText)
    match synonymTerm with
    | Some st -> FGraph.Edge.add term.Id st.Id $"synonym: {synonym.Scope.ToString()}" graph
    | None -> graph

/// Takes an OboOntology and returns an FGraph based on its terms and relations.
let toGraph (ontology : OboOntology) =
    let graph = FGraph.empty<string,OboTerm,string>
    // add Nodes first because otherwise exception "Target Node [...] does not exist" is thrown
    List.iter (addTerm graph >> ignore) ontology.Terms
    // now, add Edges
    // if this approach is too slow because of double iteration, try to implement an algorithm to simultaneously add Nodes and Edges without triggering the exception above
    ontology.Terms
    |> List.iter (
        fun t -> 
            t.IsA
            |> List.iter (addIsA graph t >> ignore)
            t.Relationships
            |> List.iter (addRelationship graph t >> ignore)
            t.Synonyms
            |> List.iter (tryAddSynonym graph t ontology >> ignore)
    )
    graph

let testOntologyGraph = toGraph testOntology
testOntologyGraph.Keys |> Seq.toList
testOntologyGraph["test:00000004"]

let testOntologyTerms2 = [
    OboTerm.Create("test:01", Name = "test1", Relationships = ["part_of test:02 ! test2"])
    OboTerm.Create("test:02", Name = "test2", Relationships = ["has_a test:01 ! test1"])
]
let testOntology2 = OboOntology.create testOntologyTerms2 []
let testOntology2Graph = toGraph testOntology2
testOntology2Graph.Keys |> Seq.toList
testOntology2Graph.Values |> Seq.toList


//let oal = eco.GetParentOntologyAnnotations(no5.Id)

//let getParents (ontology : OboOntology) (term : OboTerm) =
//    ontology.GetParentOntologyAnnotations(term.Id)
//    |> List.skip 1      // 1st item is always the term itself which we don't need here anymore
//    |> List.map (fun oa -> ontology.GetTerm(oa.TermAccessionString.ToString()))

//eco.GetChildOntologyAnnotations(no5.Id)
//eco.GetChildOntologyAnnotations(eco.Terms[1].Id)
//testOntology.GetChildOntologyAnnotations(testTerms.Head.Id)

//let getChildren (ontology : OboOntology) (term : OboTerm) =
//    ontology.GetChildOntologyAnnotations(term.Id)

//let parents = getParents eco no5

//let nodes = 
//    eco.Terms 
//    |> List.map (fun t -> LVertex(t.Id, t.Name))

//let edges = 
//    eco.Terms 
//    |> List.collect (
//        fun t -> 
//            t.IsA
//            |> List.map (
//                fun isA -> LEdge(t.Id, isA, "is_a")
//            )
//    )

//let graph = 
//    Graph.empty
//    |> Vertices.addMany nodes
//    // in directed edges the direction is from tuple item 1 to tuple item 2? (tuple item 3 is only the label, of course)
//    |> Directed.Edges.addMany edges

//graph.Count
//graph["DPEO:00000000"]
//Graph.getContext "DPEO:00000000" graph
//Graph.getContext "DPEO:00000007" graph

//let nodes2 = 
//    testOntology.Terms 
//    |> List.map (fun t -> LVertex(t.Id, t.Name))

//let edges2 =
//    testOntology.Terms 
//    |> List.collect (
//        fun t -> 
//            t.IsA
//            |> List.map (
//                fun isA -> LEdge(t.Id, isA, "is_a")
//            )
//    )

//let graph2 =
//    Graph.empty
//    |> Vertices.addMany nodes2
//    |> Directed.Edges.addMany edges2

//Graph.getContext "test:00000000" graph2
//Graph.getContext "test:00000001" graph2










// parents vs isA: parents is all isAs or partOfs recursively traced to the last ancestor while isA (or partOf in relationships list) is only the first parent



// XXXXXXXXXXX
// DEPRECATED!
// XXXXXXXXXXX

[<Literal>]
let templateTermString = """class <name>(value : string) =
    static member parents = <parents>
    static member isA = <isA>
    static member id = <id>
    static member isAnonymous = <isAnonymous>
    static member definition = <definition>
    static member altIds = <altIdsString>
    static member comment = <comment>
    static member subsets = <subsetString>
    static member synonyms = <synonymsString>
    static member unionOf = <unionOfString>
    static member xrefs = <xrefsString>
    static member intersectionOf = <intersectionOfString>
    static member relationships = <relationshipsString>
    static member isObsolete = <isObsolete>
    static member replacedby = <replacedbyString>
    static member consider = <considerString>
    static member propertyValues = <propertyValuesString>
    static member builtIn = <builtIn>
    static member createdBy = <createdBy>
    static member createionDate = <creationDate>

    member this.Value = value
"""

let foldListIntoString list =
    list
    |> List.foldi (
        fun i acc s ->
            if i = 0 then $"[\"{s}\""
            elif i = list.Length - 1 then $"{acc}; \"{s}\"]"
            else $"{acc}; \"{s}\""
    ) ""

["a1"; "a2"] |> foldListIntoString |> printfn "%s"

let myBool = false
myBool.ToString()

let boolString str =
    if str then "true" else "false"

boolString myBool

let oboTermCreateString = """OboTerm.Create(<id>, Name = <name>, IsAnonymous = <isAnonymous>, AltIds = <>, Definition = <definition>, Comment = <comment>, Subsets = <subsets>, Synonyms = <synonyms>, Xrefs = <xrefs>, IsA = <isA>, IntersectionOf = <intersectionOf>, UnionOf = <unionOf>, DisjointFrom = <disjointFrom>, Relationships = <relationships>, IsObsolete = <isObsolete>, Replacedby = <replacedBy>, Consider = <>, PropertyValues = <propertyValues>, BuiltIn = <builtIn>, CreatedBy = <createdBy>, CreationDate = <creationDate>)"""

let internal replaceTermPlaceholder template (term : OboTerm) =
    template
    |> String.replace "<name>" term.Name
    |> String.replace "<id>" term.Id
    |> String.replace "<definition>" term.Definition
    |> String.replace "<comment>" term.Comment
    |> String.replace "<createdBy>" term.CreatedBy
    |> String.replace "<creationDate>" term.CreationDate
    |> String.replace "<isAnonymous>" (boolString term.IsAnonymous)
    |> String.replace "<isObsolete>" (boolString term.IsObsolete)
    |> String.replace "<builtIn>" (boolString term.BuiltIn)
    |> String.replace "<isA>" (foldListIntoString term.IsA)
    |> String.replace "<subsets>" (foldListIntoString term.Subsets)
    |> String.replace "<synonyms>" (foldListIntoString term.Synonyms)
    |> String.replace "<xrefs>" (foldListIntoString term.Xrefs)
    |> String.replace "<intersectionOf>" (foldListIntoString term.IntersectionOf)
    |> String.replace "<unionOf>" (foldListIntoString term.UnionOf)
    |> String.replace "<disjointFrom>" (foldListIntoString term.DisjointFrom)
    |> String.replace "<relationships>" (foldListIntoString term.Relationships)
    |> String.replace "<replacedBy>" (foldListIntoString term.Replacedby)
    |> String.replace "<propertyValues>" (foldListIntoString term.PropertyValues)

let solidifyOboTerm term =
    replaceTermPlaceholder oboTermCreateString term

let insertIntoTemplate ontology term =
    let parents = getParents ontology term
    replaceTermPlaceholder templateTermString term
    |> String.replace "<parents>" ""