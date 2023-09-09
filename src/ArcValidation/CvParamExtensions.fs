namespace ArcValidation

module CvParamExtensions =

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cvp"> </param>
    let isEmpty (cvp:CvParam) =
        CvParam.getParamValue cvp
        |> ParamValue.getValue
        |> fun x -> string x = "" 

    /// <summary>
    /// 
    /// </summary>
    /// <param name="targetValue"> </param>   
    /// <param name="cvp"> </param>
    let equals (targetValue:System.IConvertible) (cvp:CvParam) =
        CvParam.getParamValue cvp
        |> ParamValue.getValue
        |> fun v -> v = targetValue 

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"> </param>   
    /// <param name="cvp"> </param>
    let tryGetValueOfCvParamAttr name (cvp:CvParam) =
        CvParam.tryGetAttribute name cvp
        |> Option.bind Param.tryCvParam
        |> Option.map CvParam.getValue

    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"> </param>   
    /// <param name="cvp"> </param>
    let errorMessageOf (error:string) (cvp:CvParam) =
        let str = new System.Text.StringBuilder()    
        str.AppendFormat("'{0}' {1}\n",  CvParam.getCvName cvp, error) |> ignore 
        match tryGetValueOfCvParamAttr "FilePath" cvp with
        | Some path ->
            str.AppendFormat(" > filePath '{0}'\n", path) |> ignore         
        | None -> ()   
        match tryGetValueOfCvParamAttr "Worksheet" cvp with
        | Some sheet ->
            str.AppendFormat(" > sheet '{0}'", sheet) |> ignore         
        | None -> ()   
        match tryGetValueOfCvParamAttr "Row" cvp with
        | Some row -> 
            str.AppendFormat(" > row '{0}'", row) |> ignore
        | None -> ()
        match tryGetValueOfCvParamAttr "Column" cvp with
        | Some column -> 
            str.AppendFormat(" > column '{0}'", column) |> ignore
        | None -> ()        
                
        match tryGetValueOfCvParamAttr "Line" cvp with
        | Some line ->
            str.AppendFormat(" > line '{0}'", line) |> ignore
        | None -> ()
        match tryGetValueOfCvParamAttr "Position" cvp with
        | Some position -> 
            str.AppendFormat(" > position '{0}'", position) |> ignore
        | None -> ()
        str.ToString()

