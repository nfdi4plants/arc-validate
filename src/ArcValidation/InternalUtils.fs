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


    module Orcid =

        /// Calculates the checksum digit of an ORCID.
        /// The calculated checksum digit must match the last character of the given ORCID.
        /// 
        /// Input parameter "digits" must be the full ORCID to check but already without all hyphens excluded.
        // modified for F# from: https://support.orcid.org/hc/en-us/articles/360006897674-Structure-of-the-ORCID-Identifier
        let checksum (digits : string) = 
            let rec loop i total =
                if i < digits.Length - 1 then
                    let digit = Char.GetNumericValue digits[i] |> int
                    loop (i + 1) ((total + digit) * 2)
                else total
            let remainder = (loop 0 0) % 11
            let result = (12 - remainder) % 11
            if result = 10 then 'X' else string result |> char

        /// Checks if a given string is a valid ORCID.
        let checkValid (input : string) =
            let rgxPat = System.Text.RegularExpressions.Regex("^\d{4}-\d{4}-\d{4}-\d{3}[0-9X]$")
            let isNum = rgxPat.Match(input).Success
            let noHyphens = String.replace "-" "" input
            isNum && checksum noHyphens = noHyphens[noHyphens.Length - 1]