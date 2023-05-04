namespace ArcValidation

[<AutoOpen>]
module internal InternalUtils =

    open System
    open FSharpAux
    open FsSpreadsheet
    open ArcGraphModel
    open ArcGraphModel.IO
    open Expecto
    open Expecto.Impl
    open System.Globalization
    open System.IO
    open System.Xml
    open System.Xml.Linq

    module String =

        /// Checks if a given string is null, empty, or consisting solely of whitespaces.
        let isNullOrWhiteSpace (str : string) =
            System.String.IsNullOrWhiteSpace str

        /// Checks if an input string option is None or, if it is Some, null, empty or consisting solely of whitespaces.
        let isNoneOrWhiteSpace str =
            Option.map isNullOrWhiteSpace str
            |> Option.defaultValue true

        /// Checks if a string is a filepath.
        let isFilepath str =
            (String.contains "/" str || String.contains "\\" str) &&
            IO.Path.GetExtension str <> ""

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


    module ArcGraphModel =

        module IO =

            module Worksheet =

                let parseColumns (worksheet : FsWorksheet) = 
                    let sheetName = Address.createWorksheetParam worksheet.Name
                    let annoTable = worksheet.Tables |> List.tryFind (fun t -> String.contains "annotationTable" t.Name)
                    match annoTable with
                    | Some t ->
                        t.Columns(worksheet.CellCollection)
                        |> Seq.toList
                        |> List.choose (fun r -> 
                            match r |> Tokenization.parseLine |> Seq.toList with
                            | [] -> None
                            | l -> Some l
                        )
                        |> List.concat
                        |> List.map (fun token ->        
                            CvAttributeCollection.tryAddAttribute sheetName token |> ignore
                            token
                        )
                    | None -> []