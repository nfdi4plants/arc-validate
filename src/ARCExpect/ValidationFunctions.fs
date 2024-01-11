namespace ARCExpect.Validate

open ControlledVocabulary
open ARCExpect
open ARCTokenization.StructuralOntology

/// <summary>
/// Validation functions to perform value-based validation on CvParams
/// </summary>
type ByValue =
    static member notEmpty (cvp:CvParam) =
        match CvParam.isEmpty cvp with
        | false -> ()
        | true ->
            cvp
            |> ErrorMessage.ofCvParam "is empty."                
            |> Expecto.Tests.failtestNoStackf "%s"

    static member notEmpty (ip : IParam) =
        match Param.isEmpty ip with
        | false -> ()
        | true ->
            ip
            |> ErrorMessage.ofIParam "is empty."                
            |> Expecto.Tests.failtestNoStackf "%s"

    static member equals (targetValue : System.IConvertible, cvp : CvParam) =
        let act = CvParam.getValue cvp
        match targetValue = act with
        | true  -> ()
        | false -> 
            cvp
            |> ErrorMessage.ofCvParam $"should equal {targetValue}."
            |> Expecto.Tests.failtestNoStackf "%s"

    static member equals (targetValue : System.IConvertible, ip : IParam) =
        let act = Param.getValue ip
        match targetValue = act with
        | true  -> ()
        | false -> 
            ip
            |> ErrorMessage.ofIParam $"should equal {targetValue}."
            |> Expecto.Tests.failtestNoStackf "%s"

    /// <summary>
    /// tests wether any of the CvParams in the given collection has the expectedValue
    /// </summary>
    /// <param name="expectedValue">the value expected to occur in at least 1 CvParam in the given collection</param>
    static member contains (expectedValue: #System.IConvertible) =
        fun (set : #seq<IParam>) -> 
            match Seq.exists (fun (ip : IParam)-> (ip.Value |> ParamValue.getValue) = (expectedValue :> System.IConvertible)) set with
            | true  -> ()
            | false ->
            expectedValue
            |> ErrorMessage.ofValue $"does not exist"
            |> Expecto.Tests.failtestNoStackf "%s"

    ///// <summary>
    ///// tests wether any of the CvParams in the given collection has the expectedValue
    ///// </summary>
    ///// <param name="expectedValue">the value expected to occur in at least 1 CvParam in the given collection</param>
    //static member contains (expectedValue: #System.IConvertible) =
    //    fun (set: #seq<CvParam>) -> 
    //        match Seq.exists (fun (cvp : CvParam)-> (cvp.Value |> ParamValue.getValue) = (expectedValue :> System.IConvertible)) set with
    //        | true  -> ()
    //        | false ->
    //        expectedValue
    //        |> ErrorMessage.ofValue $"does not exist"
    //        |> Expecto.Tests.failtestNoStackf "%s"

    /// <summary>
    /// tests wether any of the CvParams equal the expectedParam by value
    /// </summary>
    /// <param name="expectedParam">the Cvparam for which it is expected to share it's value with at least one Cvparam in the collection</param>
    static member contains (expectedParam : IParam) =
        fun (set : #seq<IParam>) -> 
            let tmp = 
                Param.getParamValue expectedParam
                |> ParamValue.getValue                
            match Seq.exists (fun (ip : IParam)-> (ip.Value |> ParamValue.getValue) = tmp) set with
            | true  -> ()
            | false ->
            expectedParam
            |> ErrorMessage.ofIParam $"does not exist"
            |> Expecto.Tests.failtestNoStackf "%s"
        

    static member isMatch (pattern:string) =
        fun (ip : IParam) ->
            let tmp = 
                Param.getParamValue ip
                |> ParamValue.getValue
                |> string
            match System.Text.RegularExpressions.Regex.IsMatch(tmp,pattern) with
            | true -> ()
            | false ->
                ip
                |> ErrorMessage.ofIParam "is invalid."               
                |> Expecto.Tests.failtestNoStackf "%s"                   


    static member isMatch (regex:System.Text.RegularExpressions.Regex) =
        fun (ip : IParam) ->
            let tmp = 
                Param.getParamValue ip
                |> ParamValue.getValue
                |> string
            match regex.IsMatch(tmp) with
            | true -> ()
            | false ->
                ip
                |> ErrorMessage.ofIParam "is invalid."              
                |> Expecto.Tests.failtestNoStackf "%s"                   
        
        
    static member isMatchBy (validator:string -> bool) =
        fun (ip : IParam) ->
            let tmp = 
                Param.getParamValue ip
                |> ParamValue.getValue
                |> string
            match validator tmp with
            | true -> ()
            | false ->
                ip
                |> ErrorMessage.ofIParam "is invalid."               
                |> Expecto.Tests.failtestNoStackf "%s"    

/// <summary>
/// Validation functions to perform term-based validation on CvParams
/// </summary>
type ByTerm =
        
    /// Compares by Term 
    static member equals (target:CvTerm) = 
        fun (ip : IParam) ->
            match (Param.getTerm ip) = target with
            | true -> ()
            | false ->
                ip
                |> ErrorMessage.ofIParam $"should equal {target}."              
                |> Expecto.Tests.failtestNoStackf "%s"                        

    /// Compares by Term 
    static member equals (target:CvParam) = 
        fun (ip : IParam) ->
            match (Param.getTerm ip) = (CvParam.getTerm target) with
            | true -> ()
            | false ->
                ip
                |> ErrorMessage.ofIParam $"should equal {target}."              
                |> Expecto.Tests.failtestNoStackf "%s"              
        
    /// <summary>
    /// tests wether any of the CvParams in the given collection is annotated with the expectedTerm
    /// </summary>
    /// <param name="expectedTerm">the term expected to occur in at least 1 CvParam in the given collection</param>
    static member contains (expectedTerm:CvTerm) =
        fun (set : #seq<IParam>) -> 
            match Seq.exists (fun e -> Param.getTerm e = expectedTerm) set with
            | true  -> ()
            | false ->
                expectedTerm
                |> ErrorMessage.ofCvTerm "is missing."
                |> Expecto.Tests.failtestNoStackf "%s"

    /// <summary>
    /// tests wether any of the CvParams equal the expectedParam by term
    /// </summary>
    /// <param name="expectedParam">the Cvparam for which it is expected to share it's term with at least one Cvparam in the collection</param>
    static member contains (expectedParam : IParam) =            
        fun (set: #seq<IParam>) -> 
            match Seq.exists (fun e -> Param.getTerm e = Param.getTerm expectedParam) set with
            | true  -> ()
            | false ->
                expectedParam
                |> ErrorMessage.ofIParam "is missing."
                |> Expecto.Tests.failtestNoStackf "%s"


/// <summary>
/// Validation functions to perform context-dependent tests on CvParams representing a special kind of object (e.g. multiple properties of a Person, an Email, etc.)
/// </summary>
type ByObject =

    static member email (ip : IParam) = 
        ip |> ByValue.isMatch StringValidationPattern.email
        ip |> ByTerm.equals INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``