[<AutoOpen>]
module AuxExt

open System
open FSharpAux
open FsSpreadsheet



module String =

    let isNullOrWhiteSpace (str : string) =
        System.String.IsNullOrWhiteSpace str

    /// Checks if an input string option is None or, if it is Some, null, empty or consisting only of whitespaces.
    let isNoneOrWhiteSpace str =
        match str with
        | None -> true
        | Some s -> String.IsNullOrWhiteSpace s

    /// Checks if a string is a filepath.
    let isFilepath str =
        (String.contains "/" str || String.contains "\\" str) &&
        IO.Path.GetExtension str <> ""

    /// Splits an file address string into a triple in the form of `sheetName * rowNumber * columnNumber`.
    let splitAddress str =
        let sheetName, res = String.split '!' str |> fun arr -> arr[0], arr[1]
        let adr = FsAddress res
        sheetName, adr.RowNumber, adr.ColumnNumber |> uint |> CellReference.indexToColAdress