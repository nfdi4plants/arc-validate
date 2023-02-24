module CheckIsaStructure

open FSharpSpreadsheetML
open FSharpAux
open ISADotNet
open ISADotNet.XLSX
open System.Collections.Generic

// ‼ remove as soon as this is released on F#Aux ‼
module Seq =
    /// Computes the intersection of two sequences.
    let intersect (seq1 : seq<'T>) seq2 : seq<'T> = 
        let smallerSeq, largerSeq =
            if Seq.length seq1 >= Seq.length seq2 then seq2, seq1
            else seq1, seq2
        let hsSs = HashSet<'T>(HashIdentity.Structural<'T>)
        smallerSeq |> Seq.iter (hsSs.Add >> ignore)
        hsSs.IntersectWith largerSeq
        hsSs

    /// Computes the outersection (known as "symmetric difference" in mathematics) of two sequences.
    let outersect seq1 (seq2 : seq<'T>) : seq<'T> = 
        let hsS1 = HashSet<'T>(HashIdentity.Structural<'T>)
        seq1 |> Seq.iter (hsS1.Add >> ignore)
        hsS1.SymmetricExceptWith seq2
        hsS1

module Array =
    /// Computes the intersection of two arrays.
    let intersect (arr1 : 'T []) (arr2 : 'T []) =
        let smallerArr, largerArr =
            if arr1.Length >= arr2.Length then arr2, arr1
            else arr1, arr2
        let hsSa = HashSet<'T>(HashIdentity.Structural<'T>)
        smallerArr |> Array.iter (hsSa.Add >> ignore)
        hsSa.IntersectWith largerArr
        Array.ofSeq hsSa

    /// Computes the outersection (known as "symmetric difference" in mathematics) of two arrays.
    let outersect arr1 (arr2 : 'T []) =
        let hsS1 = HashSet<'T>(HashIdentity.Structural<'T>)
        arr1 |> Array.iter (hsS1.Add >> ignore)
        hsS1.SymmetricExceptWith arr2
        Array.ofSeq hsS1

module List =
    /// Computes the intersection of two lists.
    let intersect (list1 : 'T list) (list2 : 'T list) =
        let smallerList, largerList =
            if list1.Length >= list2.Length then list2, list1
            else list1, list2
        let hsSl = HashSet<'T>(HashIdentity.Structural<'T>)
        smallerList |> List.iter (hsSl.Add >> ignore)
        hsSl.IntersectWith largerList
        List.ofSeq hsSl

    /// Computes the outersection (known as "symmetric difference" in mathematics) of two lists.
    let outersect (list1 : 'T list) (list2 : 'T list) = 
        let hsS1 = HashSet<'T>(HashIdentity.Structural<'T>)
        list1 |> List.iter (hsS1.Add >> ignore)
        hsS1.SymmetricExceptWith list2
        List.ofSeq hsS1


/// Checks for the existence of a metadata section of a given kind of ISA structure in an XLSX file.
let private checkForMetadataSection spreadsheet kind = (Spreadsheet.tryGetSheetBySheetName kind spreadsheet).IsSome

/// Checks for the whole metadata of a given kind of ISA structure in an XLSX file.
let private checkForMetadata spreadsheet kindFunction =
    try kindFunction spreadsheet |> Some
    with _ -> None

/// Returns the Worksheets' names from the Spreadsheet.
let private getSheetNames spreadsheet =
    Spreadsheet.getWorkbookPart spreadsheet
    |> Workbook.get
    |> Sheet.Sheets.get
    |> Sheet.Sheets.getSheets
    |> Seq.map Sheet.getName

/// Retrieves all Worksheets but those with metadata of a given kind.
let private getNotMetaDataSheets kind spreadsheet =
    getSheetNames spreadsheet
    |> Seq.filter ((<>) kind)

let private isNodeColumn nodeType sheetName spreadsheet =
    match Spreadsheet.tryGetWorksheetPartBySheetName sheetName spreadsheet with
    | Some wsp ->
        match AssayFile.Table.tryGetByDisplayNameBy (String.startsWith "annotationTable") wsp with
        | Some table -> 
            // TO DO: cell line and pos are hardcoded atm. and will be provided later™
            // TO DO: quadruple is ugly. Add Record type and create function
            Table.getColumnHeaders table |> Seq.contains nodeType, sheetName, "1", "1"      // nodeType shall be "Source Name" or "Sample Name" etc.
        | None -> false, "", "1", "1"
    | None -> false, "", "1", "1"

/// Checks if the Source Name column is present in the Worksheet with the given name in the given Spreadsheet. Returns the result and the name of the Sheet. Also returns false if the Worksheet or the Table does not exist but without the name of the Sheet.
let private isSourceNameColumnPresentInSheet sheetName spreadsheet = isNodeColumn "Source Name" sheetName spreadsheet


/// Functions for checking the ISA structure of Studies.
module Study =

    /// Checks if all existing Studies are registered in the Investigation file and if all registered Studies in the Investigation file are present in the ARC.
    let checkStudiesRegistration studiesFromFiles inves =
        let studiesFromInves = 
            match inves.Studies with
            | Some sfis -> sfis
            | None      -> []
        let studiesOutersect = List.outersect studiesFromInves studiesFromFiles
        {|
            AreStudiesRegistered    = studiesOutersect.Length <= 0
            // studies that are present in the ARC filesystem but missing in the Investigation file
            UnregisteredStudies     = studiesOutersect |> List.filter (fun so -> List.contains so studiesFromFiles)
            // studies that are present in the Investigation file but missing in the ARC filesystem
            MissingStudies          = studiesOutersect |> List.filter (fun so -> List.contains so studiesFromInves)
        |}

    /// Checks if an existing Study has a metadata section present.
    let isMetadataSectionPresent spreadsheet = checkForMetadataSection spreadsheet "Study"

    /// 
    let isMetadataPresent study = ()

    let isSourceNameColumnPresent studyPath = 
        let spreadsheet = Spreadsheet.fromFile studyPath false
        let nmds = getNotMetaDataSheets "Study" spreadsheet
        nmds
        |> Seq.map (fun sheet -> isSourceNameColumnPresentInSheet sheet spreadsheet)


/// Functions for checking the ISA structure of Assays.
module Assay =

    /// Checks if all existing Assays are registered in the Investigation file and if all registered Assays in the Investigation file are present in the ARC.
    let checkAssaysRegistration assaysFromPaths inves =
        let studiesFromInves = 
            match inves.Studies with
            | Some sfis -> sfis
            | None      -> []
        let assaysFromStudies = 
            studiesFromInves
            |> List.collect (
                fun sfi -> 
                    match sfi.Assays with
                    | Some ass -> ass
                    | None -> []
            )
        let assaysOutersect = List.outersect assaysFromStudies assaysFromPaths
        {|
            AreAssaysRegistered = assaysOutersect.Length <= 0
            UnregisteredAssays  = assaysOutersect |> List.filter (fun ao -> List.contains ao assaysFromPaths)
            MissingAssays       = assaysOutersect |> List.filter (fun ao -> List.contains ao assaysFromStudies)
        |}

    /// Checks if an existing Assay has a metadata section present.
    let isMetadataSectionPresent assayPaths = checkForMetadataSection assayPaths "Assay"

