namespace ARCExpect

open ControlledVocabulary

[<AutoOpen>]
module CVExtensions =

    type CvParam with 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cvp"> </param>
        static member isEmpty (cvp:CvParam) =
            CvParam.getParamValue cvp
            |> ParamValue.getValue
            |> fun x -> string x = "" 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetValue"> </param>   
        /// <param name="cvp"> </param>
        static member valueEquals (targetValue:System.IConvertible) (cvp:CvParam) =
            CvParam.getParamValue cvp
            |> ParamValue.getValue
            |> fun v -> v = targetValue 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"> </param>   
        /// <param name="cvp"> </param>
        static member tryGetValueOfCvParamAttr name (cvp:CvParam) =
            CvParam.tryGetAttribute name cvp
            |> Option.bind Param.tryCvParam
            |> Option.map CvParam.getValue

    type Param with 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"> </param>
        static member isEmpty (ip : IParam) =
            Param.getParamValue ip
            |> ParamValue.getValue
            |> fun x -> string x = "" 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="targetValue"> </param>   
        /// <param name="ip"> </param>
        static member valueEquals (targetValue:System.IConvertible) (ip : IParam) =
            Param.getParamValue ip
            |> ParamValue.getValue
            |> fun v -> v = targetValue 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"> </param>   
        /// <param name="ip"> </param>
        static member tryGetAttributeByTermName (ip : IParam) (name : string) =
            match ip with
            | :? CvParam as cvp -> CvParam.tryGetAttribute name cvp
            | :? UserParam as up -> UserParam.tryGetAttribute name up
            | _ -> None

        /// <summary>
        /// 
        /// </summary>
        /// <param name="term"> </param>   
        /// <param name="ip"> </param>
        static member tryGetAttribute (ip : IParam) (term : CvTerm) =
            Param.tryGetAttributeByTermName ip term.Name

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"> </param>   
        /// <param name="ip"> </param>
        static member tryGetValueOfCvParamAttr name (ip : IParam) =
            Param.tryGetAttributeByTermName ip name
            |> Option.map Param.getValue