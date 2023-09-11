namespace ARCExpect


module StringValidationPattern =

    open System
    open System.Text.RegularExpressions
    open FSharpAux
    
    let email = Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")
    let orcid = Regex(@"^\d{4}-\d{4}-\d{4}-\d{3}[0-9X]$")

    /// Functions to work with ORCID numbers.
    module Orcid =

        /// Calculates the checksum digit of an ORCID.
        /// The calculated checksum digit must match the last character of the given ORCID.
        /// 
        /// Input parameter "digits" must be the full ORCID to check but already without all hyphens excluded, e.g. `1111222233334444`.
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

        /// Checks if the given ORCID digits are in range.
        /// Current range is 0000000150000007 to 0000000350000001 and 0009000000000000 to 0009001000000000 (as of July 2023).
        /// 
        /// Input parameter "digits" must be the full ORCID to check but already without all hyphens excluded, e.g. `1111222233334444`.
        // source: https://support.orcid.org/hc/en-us/articles/360006897674-Structure-of-the-ORCID-Identifier
        let checkRange (digits : string) =
            let inNums = 
                String.replace "X" "9" digits   // debatable if sufficient. Best approach would be to raise the digit before by 1 (recursively, if it'd be 9 a.s.o.)
                |> int64
            (inNums >= 150000007L && inNums <= 350000001L) || (inNums >= 9000000000000L && inNums <= 9001000000000L)

        /// Checks if a given string is a valid ORCID.
        /// 
        /// Checks if the ORCID is in current number range and has a valid checksum digit.
        let checkValid (input : string) =            
            let isNum = orcid.Match(input).Success
            let noHyphens = String.replace "-" "" input
            isNum && checkRange noHyphens && checksum noHyphens = noHyphens[noHyphens.Length - 1]