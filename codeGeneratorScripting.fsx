#r "nuget: FsOboParser"
#r "nuget: FSharpAux"

open FsOboParser
open FSharpAux

let eco = OboOntology.fromFile true @"C:\Repos\nfdi4plants\arc-validate\ErrorClassOntology.obo"

let spaﬂTerms = [
    OboTerm.Create("spaﬂ:00000000", Name = "spaﬂ")
]

let spaﬂOntology =
    OboOntology.create spaﬂTerms []

eco.Terms.Head
let no5 = eco.Terms[5]

no5.Id

let oal = eco.GetParentOntologyAnnotations(no5.Id)

// parents vs isA: parents is all isAs or partOfs recursively traced to the last ancestor while isA (or partOf in relationships list) is only the first parent

[<Literal>]
let templateTermString = """
class <name>(value : string) =
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

let getParents (ontology : OboOntology) (term : OboTerm) =
    ontology.GetParentOntologyAnnotations(term.Id)
    |> List.skip 1      // 1st item is always the term itself which we don't need here anymore
    |> List.map (fun oa -> ontology.GetTerm(oa.TermAccessionString.ToString()))

let parents = getParents eco no5

let foldListIntoString list =
    list
    |> List.foldi (
        fun i acc s ->
            if i = 0 then $"[\"{s}\""
            elif i = list.Length - 1 then $"{acc}; \"{s}\"]"
            else $"{acc}; \"{s}\""
    ) ""

["a1"; "a2"] |> foldListIntoString |> printfn "%s"

let insertIntoTemplate ontology term =
    let parents = getParents ontology term
    templateTermString
    |> String.replace "<parents>" ""
    |> String.replace "<isA>" (foldListIntoString term.IsA)