module ontologyHelperFunctions

open Expecto
open FSharpAux

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

/// Representation of a Message as a record type. Path is mandatory, Line and Position are optional. 
/// For mere binary files and folders, Line and Position shall be None. 
/// For textfiles, Line and Position shall lead to the first character in question. 
/// For XLSX files, Line and Position together shall lead to the cell in question.
type Message = {
    Path        : string
    Line        : string option
    Position    : string option
}

/// Creates a message with the given path, and, optionally, line and position. 
/// For mere binary files and folders, line and position shall be None. 
/// For textfiles, line and position shall lead to the first character in question. 
/// For XLSX files, line and position together shall lead to the cell in question.
let createMessage path line pos = {Path = path; Line = line; Position = pos}

/// Checks if a given entity is present.
// use this for checking for files, folders, and ISA-related stuff, e.g. if all Source/Sample Names are given etc.
let isPresent actual message = 
    if actual then ()
    else failtestf "Actual entity is not present: %s" message.Path     // <- string hier ist expliziter Fehler (ohne Ort, Ort wird über message realisiert), also Fehlermeldung zum Name der Funktion
    // right now missing here: incorporation of line and position

/// Checks if a given ISA value is registered in the ISA Investigation file.
let isRegistered actual message =
    if actual then ()
    else
    failtestf "Actual value is not registered: %s" message.Path

/// Checks if a given version is valid.
// use this for e.g. CWL version (must be 1.2+)
let isValidVersion actual message =
    let parsedVersion = parseCwlVer actual
    if parsedVersion.Major >= 1 && parsedVersion.Minor >= 2 then ()
    else 
        failtestf "Actual CWL version is below required version 1.2: %s" message.Path

/// Checks if at least one of two given entities are present.
// use this for CWL check: MUST either contain tool description or workflow description
let isEitherPresent actual1 actual2 message =
    if actual1 || actual2 then ()
    else
        failtestf "Neither of the actual entities are present: %s" message.Path

/// Checks if an entity is reproducible.
// use this for checking for Run data reproducibility
let isReproducible actual message =
    if actual then ()
    else
        failtestf "Actual entity is not reproducible: %s" message.Path

/// Checks if an entity is a valid ontological term.
let isValidTerm actual message =
    if actual then ()
    else
        failtestf "Actual entity is not valid: %s" message.Path