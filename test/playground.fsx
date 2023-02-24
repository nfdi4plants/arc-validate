#r "nuget: FSharpSpreadsheetml"
#r "nuget: IsaDotNet"
#r "nuget: IsaDotNet.xlsx"
#r "nuget: FSharpAux"

open FSharpSpreadsheetML
open FSharpAux
open ISADotNet.QueryModel
open ISADotNet.XLSX

let testPath = @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\testARC28\studies\sid\isa.study.xlsx"
let testPath2 = @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\testARC30\studies\sid\isa.study.xlsx"

let testStudy = StudyFile.Study.fromFile testPath

let sss = Spreadsheet.fromFile testPath false
let sss2 = Spreadsheet.fromFile testPath2 false

/// Returns the Worksheets' names from the Spreadsheet.
// private machen
let getSheetNames spreadsheet =
    Spreadsheet.getWorkbookPart spreadsheet
    |> Workbook.get
    |> Sheet.Sheets.get
    |> Sheet.Sheets.getSheets
    |> Seq.map Sheet.getName

/// Retrieves all Worksheets but those with metadata of a given kind.
// private machen
let getNotMetaDataSheets kind spreadsheet =
    getSheetNames spreadsheet
    |> Seq.filter ((<>) kind)

// private machen
let isNodeColumn nodeType sheetName spreadsheet =
    match Spreadsheet.tryGetWorksheetPartBySheetName sheetName spreadsheet with
    | Some wsp ->
        match AssayFile.Table.tryGetByDisplayNameBy (String.startsWith "annotationTable") wsp with
        | Some table -> 
            // TO DO: cell line and pos are hardcoded atm. and will be provided later™
            // TO DO: quadruple is ugly. Add Record type and create function
            Table.getColumnHeaders table |> Seq.contains nodeType, sheetName, "1", "1"      // nodeType shall be "Source Name" or "Sample Name" etc.
        | None -> false, "", "1", "1"
    | None -> false, "", "1", "1"

// private machen
/// Checks if the Source Name column is present in the Worksheet with the given name in the given Spreadsheet. Returns the result and the name of the Sheet. Also returns false if the Worksheet or the Table does not exist but without the name of the Sheet.
let isSourceNameColumnPresentInSheet sheetName spreadsheet = isNodeColumn "Source Name" sheetName spreadsheet

let isSourceNameColumnPresent spreadsheet = 
    let nmds = getNotMetaDataSheets "Study" spreadsheet
    nmds
    |> Seq.map (fun sheet -> isSourceNameColumnPresentInSheet sheet spreadsheet)
    

sss.Close()
sss2.Close()

let areTermsValid kind spreadsheet = 
    let nmds = getNotMetaDataSheets kind spreadsheet
    Table.toSparseValueMatrix
