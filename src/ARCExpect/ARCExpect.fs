namespace ARCExpect
open Expecto

module ARCExpect =

    /// <summary>
    /// Computation expression for creating ARC validation cases.
    /// </summary>
    /// <param name="id">id of the test. can either be a guid or a string. Must be unique in the given context.</param>
    let validate (id:TestID) = 
        TestCaseBuilderSp(id)

    /// <summary>
    /// Passes if one of the given actions passes, and fails if both fail.
    /// </summary>
    /// <param name="arcExpect1"></param>
    /// <param name="arcExpect2"></param>
    let either arcExpect1 arcExpect2 = 
        try
            arcExpect1 () 
        with
        | ex1 -> 
            try
                arcExpect2 () 
            with
            | ex2 ->                          
                Expecto.Tests.failtestNoStackf "%s or %s" ex1.Message ex2.Message

module Validate =

    open ControlledVocabulary

    open ARCTokenization.StructuralOntology

    /// <summary>
    /// Validation functions to perform value-based validation on CvParams
    /// </summary>
    type ByValue() =
        static member notEmpty (cvp:CvParam) =
            match CvParam.isEmpty cvp with
            | false -> ()
            | true ->
                cvp
                |> ErrorMessage.ofCvParam "is empty."                
                |> Expecto.Tests.failtestNoStackf "%s"

        static member equals (targetValue:System.IConvertible) (cvp:CvParam) =
            let act = CvParam.getValue cvp
            match targetValue = act with
            | true  -> ()
            | false -> 
                cvp
                |> ErrorMessage.ofCvParam $"should equal {targetValue}."
                |> Expecto.Tests.failtestNoStackf "%s"

        /// <summary>
        /// tests wether any of the CvParams in the given collection has the expectedValue
        /// </summary>
        /// <param name="expectedValue">the value expected to occur in at least 1 CvParam in the given collection</param>
        static member contains (expectedValue: #System.IConvertible) =
            fun (set: #seq<CvParam>) -> 
                match Seq.exists (fun (cvp : CvParam)-> (cvp.Value |> ParamValue.getValue) = (expectedValue :> System.IConvertible)) set with
                | true  -> ()
                | false ->
                expectedValue
                |> ErrorMessage.ofValue $"does not exist"
                |> Expecto.Tests.failtestNoStackf "%s"

        /// <summary>
        /// tests wether any of the CvParams equal the expectedParam by value
        /// </summary>
        /// <param name="expectedParam">the Cvparam for which it is expected to share it's value with at least one Cvparam in the collection</param>
        static member contains (expectedParam:CvParam) =
            fun (set: #seq<CvParam>) -> 
                let tmp = 
                    CvParam.getParamValue expectedParam
                    |> ParamValue.getValue                
                match Seq.exists (fun (cvp : CvParam)-> (cvp.Value |> ParamValue.getValue) = tmp) set with
                | true  -> ()
                | false ->
                expectedParam
                |> ErrorMessage.ofCvParam $"does not exist"
                |> Expecto.Tests.failtestNoStackf "%s"
        

        static member isMatch (pattern:string) =
            fun (cvp:CvParam) ->
                let tmp = 
                    CvParam.getParamValue cvp
                    |> ParamValue.getValue
                    |> string
                match System.Text.RegularExpressions.Regex.IsMatch(tmp,pattern) with
                | true -> ()
                | false ->
                    cvp
                    |> ErrorMessage.ofCvParam "is invalid."               
                    |> Expecto.Tests.failtestNoStackf "%s"                   


        static member isMatch (regex:System.Text.RegularExpressions.Regex) =
            fun (cvp:CvParam) ->
                let tmp = 
                    CvParam.getParamValue cvp
                    |> ParamValue.getValue
                    |> string
                match regex.IsMatch(tmp) with
                | true -> ()
                | false ->
                    cvp
                    |> ErrorMessage.ofCvParam "is invalid."              
                    |> Expecto.Tests.failtestNoStackf "%s"                   
        
        
        static member isMatchBy (validator:string -> bool) =
            fun (cvp:CvParam) ->
                let tmp = 
                    CvParam.getParamValue cvp
                    |> ParamValue.getValue
                    |> string
                match validator tmp with
                | true -> ()
                | false ->
                    cvp
                    |> ErrorMessage.ofCvParam "is invalid."               
                    |> Expecto.Tests.failtestNoStackf "%s"    

    /// <summary>
    /// Validation functions to perform term-based validation on CvParams
    /// </summary>
    type ByTerm() =
        
        /// Compares by Term 
        static member equals (target:CvTerm) = 
            fun (cvp:CvParam) ->
                match (CvParam.getTerm cvp) = target with
                | true -> ()
                | false ->
                    cvp
                    |> ErrorMessage.ofCvParam $"should equal {target}."              
                    |> Expecto.Tests.failtestNoStackf "%s"                        

        /// Compares by Term 
        static member equals (target:CvParam) = 
            fun (cvp:CvParam) ->
                match (CvParam.getTerm cvp) = (CvParam.getTerm target) with
                | true -> ()
                | false ->
                    cvp
                    |> ErrorMessage.ofCvParam $"should equal {target}."              
                    |> Expecto.Tests.failtestNoStackf "%s"              
        
        /// <summary>
        /// tests wether any of the CvParams in the given collection is annotated with the expectedTerm
        /// </summary>
        /// <param name="expectedTerm">the term expected to occur in at least 1 CvParam in the given collection</param>
        static member contains (expectedTerm:CvTerm) =
            fun (set: #seq<CvParam>) -> 
                match Seq.exists (fun e -> CvParam.getTerm e = expectedTerm) set  with
                | true  -> ()
                | false ->
                    expectedTerm
                    |> ErrorMessage.ofCvTerm "is missing."
                    |> Expecto.Tests.failtestNoStackf "%s"

        /// <summary>
        /// tests wether any of the CvParams equal the expectedParam by term
        /// </summary>
        /// <param name="expectedParam">the Cvparam for which it is expected to share it's term with at least one Cvparam in the collection</param>
        static member contains (expectedParam:CvParam) =            
            fun (set: #seq<CvParam>) -> 
                match Seq.exists (fun e -> CvParam.getTerm e = CvParam.getTerm expectedParam) set with
                | true  -> ()
                | false ->
                    expectedParam
                    |> ErrorMessage.ofCvParam "is missing."
                    |> Expecto.Tests.failtestNoStackf "%s"

// <--- Functions directly in the `Validate` module come here, these are funcxtions that are either composite or do not fit in the `ByValue` or `ByTerm` modules

    let email (cvp:CvParam) = 
        cvp |> ByValue.isMatch StringValidationPattern.email
        cvp |> ByTerm.equals INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``

