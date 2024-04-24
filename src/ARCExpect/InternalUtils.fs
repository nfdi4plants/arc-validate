namespace ARCExpect


open System
open Graphoscope
open System.Text.Json
open System.Text.Json.Serialization


//// this is needed to allow ValidatorTests project to access internal modules
//[<assembly: Runtime.CompilerServices.InternalsVisibleTo("ARCValidation.Tests")>]
//do()

[<AutoOpen>]
module InternalUtils =

    open FSharpAux
    open FsSpreadsheet
    //open ARCGraphModel
    //open ARCGraphModel.IO
    open System.IO
    open OBO.NET
    open ControlledVocabulary

    /// internal json options for better F# type support in serialization (mainly for Options)
    module JsonOptions =
        let options =
            JsonFSharpOptions.Default()
                .WithSkippableOptionFields() // if option is none, do not include a property, but include it if option is some.
                .ToJsonSerializerOptions()


    module String =

        /// Checks if a given string is null, empty, or consisting solely of whitespaces.
        let isNullOrWhiteSpace (str : string) =
            String.IsNullOrWhiteSpace str

        /// Checks if an input string option is None or, if it is Some, null, empty or consisting solely of whitespaces.
        let isNoneOrWhiteSpace str =
            Option.map isNullOrWhiteSpace str
            |> Option.defaultValue true

        /// Checks if a string is a filepath.
        let isFilepath str =
            (String.contains "/" str || String.contains "\\" str) &&
            Path.GetExtension str <> ""

        /// Splits an file address string into a triple in the form of `sheetName * rowNumber * columnNumber`.
        let splitAddress str =
            let sheetName, res = String.split '!' str |> fun arr -> arr[0], arr[1]
            let adr = FsAddress res
            sheetName, adr.RowNumber, adr.ColumnNumber


    type Directory with

        /// Returns the names of files (including their paths) in the specified directory if they exist. Else returns None.
        static member TryGetFiles path =
            try Directory.GetFiles path |> Some
            with :? System.ArgumentException -> None

        /// Returns the names of files (including their paths) that match the specified search pattern in the specified directory if they
        /// exist. Else returns None.
        static member TryGetFiles(path, searchPattern) =
            try Directory.GetFiles(path, searchPattern) |> Some
            with :? System.ArgumentException -> None


    type OboTerm with

        static member toCvTerm (term : OboTerm) =
            let ref = String.takeWhile ((<>) ':') term.Id
            CvTerm.create(term.Id, term.Name, ref)


    type FGraph with

        /// Returns the key of the node in a structured ontology-FGraph that has no other nodes pointing to.
        static member getTopNodeKey (graph : FGraph<_,_,_>) =
            graph.Keys
            |> Seq.find (fun k -> FContext.successors graph[k] |> Seq.length = 0)

        /// Returns the nodedata of the given graph by using a given nodekey.
        static member getNodeData nodeKey (graph : FGraph<_,_,_>) =
            graph[nodeKey] |> fun (p,nd,s) -> nd