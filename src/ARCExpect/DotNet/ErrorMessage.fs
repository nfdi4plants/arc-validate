namespace ARCExpect

open ControlledVocabulary
open System
open System.Text

type ErrorMessage =
    
    static member ofValue (error:string) (value:#IConvertible) =

        let str = new System.Text.StringBuilder()    
        str.AppendFormat("The value '{0}' {1}\n",  value, error) |> ignore 
        str.ToString()

    static member ofCvTerm (error:string) (cvTerm: CvTerm) = 

        let str = new System.Text.StringBuilder()    
        str.AppendFormat("The term '{0}' {1}\n",  cvTerm.Name, error) |> ignore 
        //str.AppendFormat("'from {0}' with accession {1}",  cvTerm.RefUri, cvTerm.Accession) |> ignore 
        str.ToString()

    static member ofCvParam (error:string) (cvParam: CvParam) =

        let str = new StringBuilder()    
        str.AppendFormat("'{0}' {1}\n",  CvParam.getCvName cvParam, error) |> ignore 

        match CvParam.tryGetValueOfCvParamAttr "FilePath" cvParam with
        | Some path ->
            str.AppendFormat(" > filePath '{0}'\n", path) |> ignore         
        | None -> ()

        match CvParam.tryGetValueOfCvParamAttr "Worksheet" cvParam with
        | Some sheet ->
            str.AppendFormat(" > sheet '{0}'", sheet) |> ignore         
        | None -> ()

        match CvParam.tryGetValueOfCvParamAttr "Row" cvParam with
        | Some row -> 
            str.AppendFormat(" > row '{0}'", row) |> ignore
        | None -> ()

        match CvParam.tryGetValueOfCvParamAttr "Column" cvParam with
        | Some column -> 
            str.AppendFormat(" > column '{0}'", column) |> ignore
        | None -> ()        
                
        match CvParam.tryGetValueOfCvParamAttr "Line" cvParam with
        | Some line ->
            str.AppendFormat(" > line '{0}'", line) |> ignore
        | None -> ()

        match CvParam.tryGetValueOfCvParamAttr "Position" cvParam with
        | Some position -> 
            str.AppendFormat(" > position '{0}'", position) |> ignore
        | None -> ()
        str.ToString()

    static member ofIParam (error:string) (iParam: IParam) =

        let str = new StringBuilder()    
        str.AppendFormat("'{0}' {1}\n",  Param.getCvName iParam, error) |> ignore 

        match Param.tryGetValueOfCvParamAttr "FilePath" iParam with
        | Some path ->
            str.AppendFormat(" > filePath '{0}'\n", path) |> ignore         
        | None -> ()

        match Param.tryGetValueOfCvParamAttr "Worksheet" iParam with
        | Some sheet ->
            str.AppendFormat(" > sheet '{0}'", sheet) |> ignore         
        | None -> ()

        match Param.tryGetValueOfCvParamAttr "Row" iParam with
        | Some row -> 
            str.AppendFormat(" > row '{0}'", row) |> ignore
        | None -> ()

        match Param.tryGetValueOfCvParamAttr "Column" iParam with
        | Some column -> 
            str.AppendFormat(" > column '{0}'", column) |> ignore
        | None -> ()        
                
        match Param.tryGetValueOfCvParamAttr "Line" iParam with
        | Some line ->
            str.AppendFormat(" > line '{0}'", line) |> ignore
        | None -> ()

        match Param.tryGetValueOfCvParamAttr "Position" iParam with
        | Some position -> 
            str.AppendFormat(" > position '{0}'", position) |> ignore
        | None -> ()
        str.ToString()