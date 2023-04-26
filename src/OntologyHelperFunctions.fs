module OntologyHelperFunctions

open FSharpAux
open Expecto
open ArcGraphModel
open FsSpreadsheet
open FsSpreadsheet.CellReference
open ErrorMessage


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


/// Represents the result of a validation process.
type ValidationResult =
    | Success
    | Error of Message

/// Throws an error if given ValidationResult is of Error and constructs an error message by the given function.
let throwError failStringFunction result = 
    match result with
    | Success -> ()
    | Error m -> failtestf (failStringFunction m |> Printf.StringFormat<unit>)






// [DEPRECATED]

///// Checks if a given entity is present.
//// use this for checking for files, folders, and ISA-related stuff, e.g. if all Source/Sample Names are given etc.
//let isPresent actual message = 
//    if actual then ()
//    else 
//        match message.Kind with 
//        | FilesystemEntry -> failtestf "Actual entity is not present: %s" message.Path     // <- string hier ist expliziter Fehler (ohne Ort, Ort wird über message realisiert), also Fehlermeldung zum Name der Funktion
//        | Textfile -> failtestf "Actual entity is not present: %s at Line: %i, Position: %i" message.Path message.Line.Value message.Position.Value
//        | XLSXFile -> failtestf "Actual entity is not present: %s in Worksheet %s at Cell: %s%i" message.Path message.Sheet.Value (uint message.Position.Value |> indexToColAdress) message.Line.Value

///// Checks if a given ISA value is registered in the ISA Investigation file.
//let isRegistered actual message =
//    if actual then ()
//    else failtestf "Actual value is not registered: %s" message.Path

///// Checks if a given version is valid.
//// use this for e.g. CWL version (must be 1.2+)
//let isValidVersion actual message =
//    let parsedVersion = parseCwlVer actual
//    if parsedVersion.Major >= 1 && parsedVersion.Minor >= 2 then ()
//    else failtestf "Actual CWL version is below required version 1.2: %s" message.Path

///// Checks if at least one of two given entities are present.
//// use this for CWL check: MUST either contain tool description or workflow description
//let isEitherPresent actual1 actual2 message =
//    if actual1 || actual2 then ()
//    else failtestf "Neither of the actual entities are present: %s" message.Path

///// Checks if an entity is reproducible.
//// use this for checking for Run data reproducibility
//let isReproducible actual message =
//    if actual then ()
//    else failtestf "Actual entity is not reproducible: %s" message.Path

///// Checks if an entity is a valid ontological term. Only applies to XLSX files therefore can only be used in tests for such.
//let isValidTerm actual message =
//    if actual then ()
//    else
//        failtestf 
//            "Actual entity is not valid: %s in Worksheet %s at Cell%s%i" 
//            message.Path 
//            message.Sheet.Value 
//            (indexToColAdress <| uint message.Position.Value)
//            message.Line.Value