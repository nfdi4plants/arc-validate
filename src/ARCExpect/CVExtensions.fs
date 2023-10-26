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
