namespace ARCExpect



module ParamCollection =
    open ControlledVocabulary
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cvt"> </param>
    /// <param name="source"> </param>    
    let tryFindby (cvt:CvTerm) (source : seq<IParam>) =
        source
        |> Seq.choose Param.tryCvParam
        |> Seq.tryFind (fun param -> CvParam.getTerm param = cvt)

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cvt"> </param>
    /// <param name="source"> </param>          
    let findAllby (cvt:CvTerm) (source : seq<IParam>) =
        source
        |> Seq.choose Param.tryCvParam
        |> Seq.filter (fun param -> CvParam.getTerm param = cvt)