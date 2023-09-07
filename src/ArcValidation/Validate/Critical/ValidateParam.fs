namespace ArcValidation.Validate.Critical

open ArcValidation

open ArcGraphModel
open FSharpAux
open System.IO
open OntologyHelperFunctions


/// Functions to validate #IParam entities.
module Param =

    /// Validates a filepath.
    let filepath<'T when 'T :> IParam> (arcRootPath:string) (filepathParam : 'T) =
        let relFilepath = Param.getValueAsString filepathParam
        let fullpath =
            // if relative path from ARC root is provided
            if String.contains  "/" relFilepath || String.contains "\\" relFilepath then
                Path.Combine(arcRootPath, relFilepath)
            // if only filename is provided, storage in element-specific subfolder is assumed
            else
                let fileKind = filepathParam |> CvBase.getCvName
                let elementFullpath = 
                    CvParam.tryGetAttribute "Pathname" filepathParam 
                    |> Option.get 
                    |> Param.getValueAsString 
                    |> String.replace "/" "\\"
                //printfn "elementFullpath is %s" elementFullpath
                let subFolder =
                    match fileKind with
                    | "Protocol REF" -> "protocols"
                    | "Data" ->
                        if elementFullpath |> String.contains "\\assays\\" then "dataset"
                        elif elementFullpath |> String.contains "\\studies\\" then "resources"
                        else ""     // empty string is not an elegant way though `Path.Combine` ignores it
                    | _ -> ""
                //printfn "subFolder is %s" subFolder
                let efpTrunc =
                    let i = String.findIndexBack '\\' elementFullpath
                    elementFullpath[.. i - 1]
                //printfn "efpTrunc is %s" efpTrunc
                Path.Combine(efpTrunc, subFolder, relFilepath)
                |> String.replace "\\" "/"
                //|> fun r -> printfn "assumed filepath is %s" r; r
        if File.Exists fullpath then Success
        else Error (ErrorMessage.XlsxFile.createFromCvParam filepathParam)