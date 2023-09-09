namespace ArcValidation

module ArcExpect =

    open CvParamExtensions


    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"> </param>
    let test (id:TestID) = 
        TestCaseBuilderSp(id)
    

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



    type ByValue() =
        static member notEmpty (cvp:CvParam) =
            match isEmpty cvp with
            | false -> ()
            | true ->
                cvp
                |> errorMessageOf "is empty."                
                |> Expecto.Tests.failtestNoStackf "%s"

        static member equals (targetValue:System.IConvertible) (cvp:CvParam) =
            let act = CvParam.getValue cvp
            match targetValue = act with
            | true  -> ()
            | false -> 
                cvp
                |> errorMessageOf $"should equal {targetValue}."
                |> Expecto.Tests.failtestNoStackf "%s"

        static member contains (set:seq<System.IConvertible>) =
            fun (cvp:CvParam) -> 
                let tmp = 
                    CvParam.getParamValue cvp
                    |> ParamValue.getValue                
                match Seq.contains tmp set with
                | true  -> ()
                | false ->
                cvp
                |> errorMessageOf $"should equal either [{set}]"
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
                    |> errorMessageOf "is invalid."               
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
                    |> errorMessageOf "is invalid."              
                    |> Expecto.Tests.failtestNoStackf "%s"                   
        
        
        static member isMatchBy (validator:string -> boll) =
            fun (cvp:CvParam) ->
                let tmp = 
                    CvParam.getParamValue cvp
                    |> ParamValue.getValue
                    |> string
                match validator tmp with
                | true -> ()
                | false ->
                    cvp
                    |> errorMessageOf "is invalid."               
                    |> Expecto.Tests.failtestNoStackf "%s"    

    type ByTerm() =
        
        /// Compares by Term 
        static member equals (target:CvTerm) = 
            fun (cvp:CvParam) ->
                match (CvParam.getTerm cvp) = target with
                | true -> ()
                | false ->
                     cvp
                    |> errorMessageOf $"should equal {target}."              
                    |> Expecto.Tests.failtestNoStackf "%s"                        

        /// Compares by Term 
        static member equals (target:CvParam) = 
            fun (cvp:CvParam) ->
                match (CvParam.getTerm cvp) = (CvParam.getTerm target) with
                | true -> ()
                | false ->
                     cvp
                    |> errorMessageOf $"should equal {target}."              
                    |> Expecto.Tests.failtestNoStackf "%s"              
        
        static member exists (set:seq<CvTerm>) =
            fun (cvp:CvParam) -> 
                let term = CvParam.getTerm cvp
                match Seq.contains term set with 
                | true -> ()
                | false ->
                    cvp
                    |> errorMessageOf "is missing."
                    |> Expecto.Tests.failtestNoStackf "%s"                   

        /// Compares by Term 
        static member exists (set:seq<CvParam>) =            
            fun (cvp:CvParam) -> 
                let term = CvParam.getTerm cvp              
                match Seq.exists (fun e -> CvParam.getTerm e = term) set  with
                | true  -> ()
                | false ->
                    cvp
                    |> errorMessageOf "is missing."
                    |> Expecto.Tests.failtestNoStackf "%s"

