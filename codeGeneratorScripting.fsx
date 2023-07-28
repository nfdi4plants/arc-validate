#r "nuget: FsOboParser"
#r "nuget: FSharpAux"
#r "nuget: FSharp.FGL"

open FsOboParser
open FSharpAux
open FSharp.FGL

let eco = OboOntology.fromFile true @"C:\Repos\nfdi4plants\arc-validate\ErrorClassOntology.obo"

eco.Terms.Head
let no5 = eco.Terms[5]

no5.Id

let nodes = 
    eco.Terms 
    |> List.map (fun t -> LVertex(t.Id, t.Name))

let edges = 
    eco.Terms 
    |> List.collect (
        fun t -> 
            t.IsA
            |> List.map (
                fun isA -> LEdge(t.Id, isA, "is_a")
            )
    )

let graph = 
    Graph.empty
    |> Vertices.addMany nodes
    // in directed edges the direction is from tuple item 1 to tuple item 2? (tuple item 3 is only the label, of course)
    |> Directed.Edges.addMany edges

graph.Count
graph["DPEO:00000000"]
Graph.getContext "DPEO:00000000" graph
Graph.getContext "DPEO:00000007" graph


let testTerms = [
    OboTerm.Create("test:00000000", Name = "test")
    OboTerm.Create("test:00000001", Name = "test2a", IsA = ["test:00000000"])
    OboTerm.Create("test:00000002", Name = "test3", IsA = ["test:00000001"])
    OboTerm.Create("test:00000003", Name = "test2b", IsA = ["test:00000000"])
]

let testOntology = OboOntology.create testTerms []

let nodes2 = 
    testOntology.Terms 
    |> List.map (fun t -> LVertex(t.Id, t.Name))

let edges2 =
    testOntology.Terms 
    |> List.collect (
        fun t -> 
            t.IsA
            |> List.map (
                fun isA -> LEdge(t.Id, isA, "is_a")
            )
    )

let graph2 =
    Graph.empty
    |> Vertices.addMany nodes2
    |> Directed.Edges.addMany edges2

Graph.getContext "test:00000000" graph2
Graph.getContext "test:00000001" graph2




let oal = eco.GetParentOntologyAnnotations(no5.Id)

let getParents (ontology : OboOntology) (term : OboTerm) =
    ontology.GetParentOntologyAnnotations(term.Id)
    |> List.skip 1      // 1st item is always the term itself which we don't need here anymore
    |> List.map (fun oa -> ontology.GetTerm(oa.TermAccessionString.ToString()))

eco.GetChildOntologyAnnotations(no5.Id)
eco.GetChildOntologyAnnotations(eco.Terms[1].Id)
testOntology.GetChildOntologyAnnotations(testTerms.Head.Id)

let getChildren (ontology : OboOntology) (term : OboTerm) =
    ontology.GetChildOntologyAnnotations(term.Id)

let parents = getParents eco no5

let templateModuleString = """module <name> =

"""

let templateTypeString = """type <name> =
    static member name = "<name>"
"""







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