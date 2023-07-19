#r "nuget: FSharpAux"
#r @"C:\Repos\nfdi4plants\ArcGraphModel\src\ArcGraphModel\bin\Debug\net6.0\ArcGraphModel.dll"
//#r @"C:\Repos\CSBiology\FsSpreadsheet\src\FsSpreadsheet\bin\Debug\netstandard2.0\FsSpreadsheet.dll"
//#r @

// TO DO: write validation tests that WORK ON CVPARAMS (list/array) instead of graph!

// look for a function that checks if a Raw/Derived Data file is present in the ARC (look in arc-validate)
// look if Input/Output columns have empty data cells




open System
open FSharpAux

/// Calculates the checksum digit of an ORCID.
/// The calculated checksum digit must match the last character of the given ORCID.
/// 
/// Input parameter "digits" must be the full ORCID to check but already without all hyphens excluded.
let checksum (digits : string) = 
    //let digits = String.replace "-" "" "0000-0003-3925-6778"
    let rec loop i total =
        if i < digits.Length - 1 then
            let digit = Char.GetNumericValue digits[i] |> int
            loop (i + 1) ((total + digit) * 2)
        else total
    let remainder = (loop 0 0) % 11
    let result = (12 - remainder) % 11
    if result = 10 then 'X' else string result |> char
    //let mutable total = 0
    //for i = 0 to digits.Length - 2 do
    //    let digit = Char.GetNumericValue digits[i] |> int
    //    total <- (total + digit) * 2
    //let remainder = total % 11
    //let result = (12 - remainder) % 11
    //if result = 10 then "X" else string result

checksum (String.replace "-" "" "0000-0002-8241-5300")
checksum (String.replace "-" "" "0000-0003-3925-6778")

/// Checks if a given string is a valid ORCID.
let checkValid (input : string) =
    let rgxPat = System.Text.RegularExpressions.Regex("^\d{4}-\d{4}-\d{4}-\d{3}[0-9X]$")
    let isNum = rgxPat.Match(input).Success
    let noHyphens = String.replace "-" "" input
    isNum && checksum noHyphens = noHyphens[noHyphens.Length - 1]

checkValid "0000-0002-8241-5300"
checkValid "0000-0003-3925-6778"
checkValid "0123-4567-8910-1112"

open ArcGraphModel
open ArcGraphModel.ArcType
open FSharpAux
open FsSpreadsheet

let address = [CvParam("", "Address", "", ParamValue.Value "annotationTable1!A2") |> fun c -> c :> IParam]
let address2 = [CvParam("", "Address", "", ParamValue.Value "annotationTable1!B2") |> fun c -> c :> IParam]
let address3 = [CvParam("", "Address", "", ParamValue.Value "annotationTable1!D5") |> fun c -> c :> IParam]
let testCvP3' = CvParam("ISA:0001", "Sample Name", "ISA", ParamValue.Value "", address3 |> List.map (fun c -> c :> IParam))
address.Head :> IParamBase |> ParamBase.getValue |> string
//address.Head :> IParamBase |> ParamBase.getValue :?> string

let testCvP = CvParam("ISA:0000", "Source Name", "ISA", ParamValue.Value "eppi1", address)
let testCvP2 = CvParam("ISA:0000", "Source Name", "ISA", ParamValue.Value "", address2)
let testCvP3 = CvParam("ISA:0001", "Sample Name", "ISA", ParamValue.Value "", address3)
testCvP["Address"] :> IParamBase |> ParamBase.getValue
testCvP :> IParamBase |> ParamBase.getValue



returnEmptyInputCells [testCvP; testCvP2; testCvP3]
returnEmptyOutputCells [testCvP; testCvP2; testCvP3]

let address4 = [CvParam("", "Address", "", ParamValue.Value "annotationTable1!C2")]
let ddfp = @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\testARC30\assays\aid\dataset\myDerivedDataFile1.txt"
let testCvP4 = CvParam("ISA:0002", "Derived Data File", "ISA", ParamValue.Value ddfp, address4)
let address5 = [CvParam("", "Address", "", ParamValue.Value "annotationTable1!C3")]
let ddfp2 = @"C:\Users\olive\OneDrive\CSB-Stuff\NFDI\testARC30\assays\aid\dataset\myDerivedDataFile2.txt"
let testCvP5 = CvParam("ISA:0002", "Derived Data File", "ISA", ParamValue.Value ddfp2, address5)

BuildingBlockType.tryOfString (testCvP5 :> ICvBase).Name

returnNonExistentFileNames [testCvP; testCvP4; testCvP5]