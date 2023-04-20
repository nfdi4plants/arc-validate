module OntologyHelperFunctions

open FSharpAux
open FSharpSpreadsheetML
open Expecto
open ArcGraphModel
open FsSpreadsheet


/// Representation of a CWL version as a record type.
type CWLVer = {
    Major: int
    Minor: int
    Patch: int option
}

/// Takes a CWL version as a string and parses it into a CWLVer record.
let private parseCwlVer cwlVer =
    let res = String.split '.' cwlVer
    {
        Major = int res[0]
        Minor = int res[1]
        Patch = try int res[2] |> Some with _ -> None
    }

type MessageKind =
    | FilesystemEntry
    | Textfile
    | XLSXFile

/// Representation of a Message. Path is mandatory, Line and Position are optional. 
///
/// For mere binary files and folders and all files listed below when their content does not matter, Line, Position, and Sheet shall always be None. MessageKind is `FilesystemEntry`.
///
/// For textfiles when content matters, Line and Position shall lead to the first character in question. Sheet shall be None. MessageKind is `Textfile`.
///
/// For XLSX files when content matters, Line and Position together shall lead to the cell in question. Sheet shall be the name of the concerning Worksheet. MessageKind is XLSXFile.
type Message = {
    Path        : string
    Line        : int option
    Position    : string option
    Sheet       : string option
    Kind        : MessageKind
}

/// Creates a message with the given path, and, optionally, line and position. 
/// 
/// For mere binary files and folders and all files listed below when their content does not matter, Line, Position, and Sheet shall always be None. MessageKind is `FilesystemEntry`.
///
/// For textfiles when content matters, Line and Position shall lead to the first character in question. Sheet shall be None. MessageKind is `Textfile`.
///
/// For XLSX files when content matters, Line and Position together shall lead to the cell in question. Sheet shall be the name of the concerning Worksheet. MessageKind is XLSXFile.
let createMessage path line pos sheet kind = {Path = path; Line = line; Position = pos; Sheet = sheet; Kind = kind}

/// Checks if a given entity is present.
// use this for checking for files, folders, and ISA-related stuff, e.g. if all Source/Sample Names are given etc.
let isPresent actual message = 
    if actual then ()
    else 
        match message.Kind with 
        | FilesystemEntry -> failtestf "Actual entity is not present: %s" message.Path     // <- string hier ist expliziter Fehler (ohne Ort, Ort wird über message realisiert), also Fehlermeldung zum Name der Funktion
        | Textfile -> failtestf "Actual entity is not present: %s at Line: %i, Position: %s" message.Path message.Line.Value message.Position.Value
        | XLSXFile -> failtestf "Actual entity is not present: %s in Worksheet %s at Cell: %s%i" message.Path message.Sheet.Value message.Position.Value message.Line.Value

/// Checks if a given ISA value is registered in the ISA Investigation file.
let isRegistered actual message =
    if actual then ()
    else failtestf "Actual value is not registered: %s" message.Path

/// Checks if a given version is valid.
// use this for e.g. CWL version (must be 1.2+)
let isValidVersion actual message =
    let parsedVersion = parseCwlVer actual
    if parsedVersion.Major >= 1 && parsedVersion.Minor >= 2 then ()
    else failtestf "Actual CWL version is below required version 1.2: %s" message.Path

/// Checks if at least one of two given entities are present.
// use this for CWL check: MUST either contain tool description or workflow description
let isEitherPresent actual1 actual2 message =
    if actual1 || actual2 then ()
    else failtestf "Neither of the actual entities are present: %s" message.Path

/// Checks if an entity is reproducible.
// use this for checking for Run data reproducibility
let isReproducible actual message =
    if actual then ()
    else failtestf "Actual entity is not reproducible: %s" message.Path

/// Checks if an entity is a valid ontological term. Only applies to XLSX files therefore can only be used in tests for such.
let isValidTerm actual message =
    if actual then ()
    else
        failtestf 
            "Actual entity is not valid: %s in Worksheet %s at Cell%s%i" 
            message.Path 
            message.Sheet.Value 
            (CellReference.indexToColAdress <| uint message.Position.Value)
            message.Line.Value

//let isValidPerson actual nessage =
    


/// First part of an Error message: Describes why something did not match the requirements.
module FailStrings =

    module FilesystemEntry =

        let isPresent = "Actual entity is not present"

        let isEitherPresent = "Neither of the actual entities are present"

        let isRegistered = "Actual value is not registered"

        let isValidTerm = "Actual term is not valid"

        let isValidVersion = "Actual CWL version is below required version 1.2"

        let isReproducible = "Actual entity is not reproducible"


/// Represents the result of a validation process.
type ValidationResult =
    | Success
    | Error of Message

let throwError result = 
    match result with
    | Success -> ()
    | Error m -> failtestf

/// Checks if a string is a filepath.
let isFilepath str =
    (String.contains "/" str || String.contains "\\" str) &&
    System.IO.Path.GetExtension str <> ""

/// Splits an address string into a triple inf the form of `sheetName * rowNumber * columnNumber`.
let splitAddress str =
    let sheetName, res = String.split '!' str |> fun arr -> arr[0], arr[1]
    let adr = FsAddress res
    sheetName, adr.RowNumber, adr.ColumnNumber |> uint |> CellReference.indexToColAdress

/// Functions to validate #ICvBase entities.
module Validate =

    let person<'T when 'T :> CvContainer> (person : 'T) =
    //let person (person : CvContainer) =
        let firstName = person |> Dictionary.tryFind "FirstName"
        //let firstName = person |> Dictionary.tryFind "FirstName"
        let lastName = person |> Dictionary.tryFind "LastName"
        //let lastName = person |> Dictionary.tryFind "LastName"
        let message = 
            let sheet, line, pos =
                person["Address"] 
                |> Seq.head 
                |> CvBase.getCvName
                |> splitAddress
            let path = 
                person["Path"] 
                |> Seq.head 
                //|> CvBase.getCvName
                :?> CvParam
                |> ParamBase.getValue
                |> string
            createMessage path (Some line) (Some pos) (Some sheet) XLSXFile
        match firstName, lastName with
        | None, _ -> Error message
        | _, None -> Error message
        | Some fn, Some ln ->
            match Seq.isEmpty fn, Seq.isEmpty ln with
            | false, true -> Error message
            | true, false -> Error message
            | _ ->
                match Seq.head fn |> CvBase.getCvName, Seq.head ln |> CvBase.getCvName with
                | "", _ -> Error message
                | _, "" -> Error message
                | _ -> Success

    //let persons (persons : CvContainer list) =
    //    persons
    //    |> List.map (person >> 

    let filepath<'T when 'T :> CvParam> (filepath : 'T) message =
        