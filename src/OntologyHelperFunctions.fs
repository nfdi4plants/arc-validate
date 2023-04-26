module OntologyHelperFunctions

open FSharpAux
open Expecto
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