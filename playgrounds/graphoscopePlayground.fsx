//#I "../../fslaborg/Graphoscope/src/Graphoscope/bin/Debug/netstandard2.0"
#I "../../omaus/Graphoscope/src/Graphoscope/bin/Debug/netstandard2.0"
#r "Graphoscope.dll"
#r "nuget: OBO.NET"


open Graphoscope
open OBO.NET


let testTerms = [
    OboTerm.Create("test:00000000", Name = "test")
    OboTerm.Create("test:00000001", Name = "test1a", IsA = ["test:00000000"])
    OboTerm.Create("test:00000002", Name = "test2", IsA = ["test:00000001"])
    OboTerm.Create("test:00000003", Name = "test1b", IsA = ["test:00000000"])
]

let testOntology = OboOntology.create testTerms []

let graph2 = FGraph.empty<string,string,string>

testOntology.Terms 
|> List.map (fun t -> FGraph.Node.add t.Id t.Name graph2)

testOntology.Terms 
|> List.collect (
    fun t -> 
        t.IsA
        |> List.map (
            fun isA -> FGraph.Edge.add t.Id isA "is_a" graph2
        )
)

let result = Algorithms.BFS.Compute("test:00000003", graph2) |> List.ofSeq
let result2 = Algorithms.BFS.Compute("test:00000002", graph2) |> List.ofSeq

//let graph3 = FGraph.ofSeq (testTerms |> List.map (fun t -> t.))

let errorOntology = OboOntology.fromFile false (System.IO.Path.Combine(__SOURCE_DIRECTORY__, "ErrorClassOntology.obo"))

let graph3 = 
    let g = FGraph.empty<string,string,string>
    errorOntology.Terms 
    |> List.map (fun t -> FGraph.Node.add t.Id t.Name g)
    |> ignore
    errorOntology.Terms 
    |> List.collect (
        fun t -> 
            t.IsA
            |> List.map (
                fun isA -> FGraph.Edge.add t.Id isA "is_a" g
            )
    )
    |> ignore
    g

let result3 = Algorithms.BFS.Compute("DPEO:00000018", graph3) |> List.ofSeq
let result4 = Algorithms.BFS.Compute("DPEO:00000000", graph3) |> List.ofSeq

let graph4 = 
    let g = FGraph.empty<string,string,string>
    errorOntology.Terms 
    |> List.map (fun t -> FGraph.Node.add t.Id t.Name g)
    |> ignore
    errorOntology.Terms 
    |> List.collect (
        fun t -> 
            t.IsA
            |> List.map (
                fun isA -> FGraph.Edge.add isA t.Id "has_a" g
            )
    )
    |> ignore
    g

graph4 |> FGraph.mapContexts (fun t -> t)

let result5 = Algorithms.BFS.Compute("DPEO:00000018", graph4) |> List.ofSeq
let result6 = Algorithms.BFS.Compute("DPEO:00000000", graph4) |> List.ofSeq


let graph5 = FGraph.reverseEdges graph2

let result7 = Algorithms.BFS.Compute("test:00000000", graph5) |> List.ofSeq

let graph6 = FGraph.reverseEdges graph3

let result8 = Algorithms.BFS.Compute("DPEO:00000018", graph6) |> List.ofSeq
let result9 = Algorithms.BFS.Compute("DPEO:00000000", graph6) |> List.ofSeq

result8 = result5
result9 = result6