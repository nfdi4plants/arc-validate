namespace ARCExpect

open ControlledVocabulary
open ARCExpect
open ARCTokenization.StructuralOntology

/// <summary>
/// A collection of commonly used predicates for validation
/// </summary>
module Predicates =
    
    type Param = 

        static member HasAccession (param : #IParam) = param.Accession <> ""

        static member HasName (param : #IParam) = param.Name <> ""

        static member HasReferenceOntology (param : #IParam) = param.RefUri <> ""

        static member HasTerm (param : #IParam) = 
            Param.HasAccession param
            && Param.HasName param
            && Param.HasReferenceOntology param

        static member HasSimpleValue (param : #IParam) =
            match param.Value with
            | ParamValue.Value _ -> true | _ -> false

        static member HasCvValue (param : #IParam) =
            match param.Value with
            | ParamValue.CvValue _ -> true | _ -> false

        static member HasUnitizedValue (param : #IParam) =
            match param.Value with
            | ParamValue.WithCvUnitAccession _ -> true | _ -> false
        
        static member ParamValueIsEqualTo (expectedParamValue : ParamValue) (param : #IParam) = param.Value = expectedParamValue

        static member SimpleValueIsEqualTo (expectedValue : System.IConvertible) (param : #IParam) =
            match param.Value with
            | ParamValue.Value v -> v = expectedValue
            | _ -> false

        static member CvValueIsEqualTo (expectedTerm:CvTerm) (param : #IParam) =
            match param.Value with
            | ParamValue.CvValue term -> term = expectedTerm
            | _ -> false

        static member UnitizedValueIsEqualTo (expectedValue : System.IConvertible) (expectedUnit:CvUnit) (param : #IParam) =
            match param.Value with
            | ParamValue.WithCvUnitAccession (value,unit) -> value = expectedValue && unit = expectedUnit
            | _ -> false


/// <summary>
/// Top level API for formulating expectations as validation cases 
/// </summary>
[<RequireQualifiedAccess>]
module Validate =
    
    /// <summary>
    /// Validation functions to perform on any type implementing the `IParam` interface.
    /// </summary>
    type Param = 

        /// <summary>
        /// Validates that the value of the given Param is not empty (meaning it is not an empty string: "").
        /// </summary>
        /// <param name="param">The param to validate</param>
        static member ValueIsNotEmpty (param : #IParam) =
            match Param.isEmpty param with
            | false -> ()
            | true ->
                param
                |> ErrorMessage.ofIParam "is empty."                
                |> Expecto.Tests.failtestNoStackf "%s"

        /// <summary>
        /// Validates if the value of the given Param is equal to the expected value.
        /// </summary>
        /// <param name="expectedValue">The expected value to validate against</param>
        /// <param name="param">The param to validate</param>
        static member ValueIsEqualTo (targetValue : System.IConvertible) (param : #IParam) =
            let act = Param.getValue param
            match targetValue = act with
            | true  -> ()
            | false -> 
                param
                |> ErrorMessage.ofIParam $"should equal {targetValue}."
                |> Expecto.Tests.failtestNoStackf "%s"

        /// <summary>
        /// Validates if the term of the given Param is equal to the expected term.
        /// </summary>
        /// <param name="expectedTerm">The expected term to validate against</param>
        /// <param name="param">The param to validate</param>
        static member TermIsEqualTo (expectedTerm:CvTerm) (param : #IParam) =
            match (Param.getTerm param) = expectedTerm with
            | true -> ()
            | false ->
                param
                |> ErrorMessage.ofIParam $"should equal {expectedTerm}."              
                |> Expecto.Tests.failtestNoStackf "%s"                        

        /// <summary>
        /// Validates if the value of the given Param matches a regex pattern.
        /// </summary>
        /// <param name="pattern">The regex pattern that the value should match</param>
        /// <param name="param">The param to validate</param>
        static member ValueMatchesPattern (pattern:string) (param : #IParam) =
            let tmp = 
                Param.getParamValue param
                |> ParamValue.getValue
                |> string
            match System.Text.RegularExpressions.Regex.IsMatch(tmp,pattern) with
            | true -> ()
            | false ->
                param
                |> ErrorMessage.ofIParam "is invalid."               
                |> Expecto.Tests.failtestNoStackf "%s"                   

        /// <summary>
        /// Validates if the value of the given Param matches a regex.
        /// </summary>
        /// <param name="pattern">The regex that the value should match</param>
        /// <param name="param">The param to validate</param>
        static member ValueMatchesRegex (regex:System.Text.RegularExpressions.Regex) (param : #IParam) =
            let tmp = 
                Param.getParamValue param
                |> ParamValue.getValue
                |> string
            match regex.IsMatch(tmp) with
            | true -> ()
            | false ->
                param
                |> ErrorMessage.ofIParam "is invalid."              
                |> Expecto.Tests.failtestNoStackf "%s"                   

        /// <summary>
        /// Validates if the value of the given Param satisfies a predicate (meaning a function that for a given Param returns either true or false)
        /// </summary>
        /// <param name="predicate">The predicate that the Param should satisfy</param>
        /// <param name="param">The param to validate</param>
        static member ValueSatisfiesPredicate (predicate: System.IConvertible -> bool) (param : #IParam) =
            let tmp = 
                Param.getParamValue param
                |> ParamValue.getValue

            if not (predicate tmp) then
                param
                |> ErrorMessage.ofIParam "is invalid."
                |> Expecto.Tests.failtestNoStackf "%s"

        /// <summary>
        /// Validates if the given Param satisfies a predicate (meaning a function that for a given Param returns either true or false)
        /// </summary>
        /// <param name="predicate">The predicate that the Param should satisfy</param>
        /// <param name="param">The param to validate</param>
        static member SatisfiesPredicate (predicate: IParam -> bool) (param : #IParam) =
            if not (predicate param) then
                param
                |> ErrorMessage.ofIParam "is invalid."
                |> Expecto.Tests.failtestNoStackf "%s"

    /// <summary>
    /// Validation functions to perform on a collection containing any type implementing the `IParam` interface.
    /// </summary>
    type ParamCollection =

        /// <summary>
        /// Generic method to validate wether a collection of IParams satisfies any kind of predicate.
        /// </summary>
        /// <summary>
        /// Validates whether a given predicate function returns true for the sequence of IParams.
        /// </summary>
        /// <param name="predicate">Function that projects the whole sequence.</param>
        /// <param name="paramCollection">The param collection to validate.</param>
        static member SatisfiesPredicate (predicate :#seq<#IParam> -> bool) (paramCollection : #seq<#IParam>) =
            match predicate paramCollection with
            | true  -> ()
            | false ->
                ErrorMessage.ofValue $"The does not satisfy the predicate." "paramCollection"
                |> Expecto.Tests.failtestNoStackf "%s"

        /// <summary>
        /// Validates if at least one Param with the expected value in the given collection exists.
        /// </summary>
        /// <param name="expectedValue">the value expected to occur in at least 1 Param in the given collection</param>
        /// <param name="paramCollection">The param collection to validate</param>
        static member ContainsItemWithValue (expectedValue : #System.IConvertible) (paramCollection : #seq<#IParam>) =
            match Seq.exists (fun (param : #IParam)-> (param.Value |> ParamValue.getValue) = (expectedValue :> System.IConvertible)) paramCollection with
            | true  -> ()
            | false ->
                expectedValue
                |> ErrorMessage.ofValue $"does not exist"
                |> Expecto.Tests.failtestNoStackf "%s"

        /// <summary>
        /// Validates if at least one Param with the expected term in the given collection exists.
        /// </summary>
        /// <param name="expectedTerm">the term expected to occur in at least 1 Param in the given collection</param>
        /// <param name="paramCollection">The param collection to validate</param>
        static member ContainsItemWithTerm (expectedTerm : CvTerm) (paramCollection: #seq<#IParam>) =
            match Seq.exists (fun e -> Param.getTerm e = expectedTerm) paramCollection with
            | true  -> ()
            | false ->
                expectedTerm
                |> ErrorMessage.ofCvTerm $"does not exist"
                |> Expecto.Tests.failtestNoStackf "%s"

        /// <summary>
        /// Validates if the given Param is contained in the given collection át least once.
        /// </summary>
        /// <param name="expectedParam">the param expected to occur at least once in the given collection</param>
        /// <param name="paramCollection">The param collection to validate</param>
        static member ContainsItem (expectedParam : #IParam) (paramCollection : #seq<#IParam>) =

            let tmp = 
                Param.getParamValue expectedParam
                |> ParamValue.getValue
            // this is incomplete and does not exactly perform what the name advertises 
            // (e.g., it only checks value but not term, or even if the Params are Cv or User)
            match Seq.exists (fun (param : #IParam)-> (param.Value |> ParamValue.getValue) = tmp) paramCollection with
            | true  -> ()
            | false ->
            expectedParam
            |> ErrorMessage.ofIParam $"does not exist"
            |> Expecto.Tests.failtestNoStackf "%s"


    /// <summary>
    /// Validates wether the given Param's value is an email that matches a pre-defined regex pattern ("^[^@\s]+@[^@\s]+\.[^@\s]+$") 
    /// and wether the param is annotated with the term `Investigation Person Email`
    /// </summary>
    /// <param name="param"></param>
    let email (param : #IParam) = 
        param |> Param.ValueMatchesRegex StringValidationPattern.email
        param |> Param.TermIsEqualTo INVMSO.``Investigation Metadata``.``INVESTIGATION CONTACTS``.``Investigation Person Email``

    