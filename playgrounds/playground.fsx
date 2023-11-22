#r "nuget: FSharpAux"
#r "nuget: Expecto"
#r @"C:\Repos\nfdi4plants\ArcGraphModel\src\ArcGraphModel\bin\Debug\net6.0\ArcGraphModel.dll"
//#r @"C:\Repos\CSBiology\FsSpreadsheet\src\FsSpreadsheet\bin\Debug\netstandard2.0\FsSpreadsheet.dll"
//#r @

// TO DO: write validation tests that WORK ON CVPARAMS (list/array) instead of graph!

// look for a function that checks if a Raw/Derived Data file is present in the ARC (look in arc-validate)
// look if Input/Output columns have empty data cells




open System
open FSharpAux
open Expecto

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

let noHyphens orcid = String.replace "-" "" orcid

checksum (String.replace "-" "" "0000-0002-8241-5300")
checksum (String.replace "-" "" "0000-0003-3925-6778")
let dummyOrcidTotallyWrong = "abcd-efgh-ijkl-mnox"
(String.replace "-" "" >> checksum) dummyOrcidTotallyWrong
let dummyOrcidSumWrong = "0123-4567-8910-1112"
(String.replace "-" "" >> checksum) dummyOrcidSumWrong

let lastDigit = dummyOrcidSumWrong[dummyOrcidSumWrong.Length - 1]
Expect.isTrue ((noHyphens >> checksum) dummyOrcidSumWrong = lastDigit) "ORCID checksum is correct (though it mustn't)"

let checkRange (digits : string) =
    let inNums = String.replace "X" "9" digits |> int64
    (inNums >= 150000007L && inNums <= 350000001L) || (inNums >= 9000000000000L && inNums <= 9001000000000L)

let checkValid (input : string) =
    let rgxPat = System.Text.RegularExpressions.Regex("^\d{4}-\d{4}-\d{4}-\d{3}[0-9X]$")
    let isNum = rgxPat.Match(input).Success
    let noHyphens = String.replace "-" "" input
    isNum && checkRange noHyphens && checksum noHyphens = noHyphens[noHyphens.Length - 1]

let invNum1 = "1234-5678-9101-1121"
let invNum2 = "0123-4567-8910-1112"
let invNum3 = "0000-1111-2222-3333"
let invNum4 = "0123-1234-2345-3456"

checkValid invNum1
checkValid invNum2
checkValid invNum3
checkValid invNum4

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