#I "../../fslaborg/Graphoscope/src/Graphoscope/bin/Debug/netstandard2.0"
#r "Graphoscope.dll"
#r "nuget: FsOboParser"


open Graphoscope
open FsOboParser


let testTerms = [
    OboTerm.Create("test:00000000", Name = "test")
    OboTerm.Create("test:00000001", Name = "test1a", IsA = ["test:00000000"])
    OboTerm.Create("test:00000002", Name = "test2", IsA = ["test:00000001"])
    OboTerm.Create("test:00000003", Name = "test1b", IsA = ["test:00000000"])
]

let testOntology = OboOntology.create testTerms []

let graph2 = FGraph.empty

let nodes2 = 
    testOntology.Terms 
    |> List.map (fun t -> FGraph.Node.add t.Id t.Name graph2)

let edges2 =
    testOntology.Terms 
    |> List.collect (
        fun t -> 
            t.IsA
            |> List.map (
                fun isA -> FGraph.Edge.add t.Id isA "is_a" graph2
            )
    )

let result = Algorithms.BFS.Compute("test:00000003", graph2)

FGraph.ofSeq

result |> List.ofSeq