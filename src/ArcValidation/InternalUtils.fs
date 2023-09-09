namespace ArcValidation

open System


// this is needed to allow ValidatorTests project to access internal modules
[<assembly: Runtime.CompilerServices.InternalsVisibleTo("ARCValidation.Tests")>]
do()

[<AutoOpen>]
module internal InternalUtils =

    open FSharpAux
    open FsSpreadsheet
    //open ArcGraphModel
    //open ArcGraphModel.IO
    open System.IO
    open FsOboParser
    open ControlledVocabulary

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