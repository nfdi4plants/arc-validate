namespace Validate

open ArcGraphModel
open FSharpAux
open System.IO
open OntologyHelperFunctions
open Defaults

/// Functions to validate #IParam entities.
module Param =

    /// Validates a filepath.
    let filepath<'T when 'T :> IParam> (filepathParam : 'T) =
        let relFilepath = Param.getValueAsString filepathParam
        let fullpath =
            // if relative path from ARC root is provided
            if String.contains  "/" relFilepath || String.contains "\\" relFilepath then
                Path.Combine(arcPaths.ArcRootPath, relFilepath)
            // if only filename is provided, storage in dataset folder is assumed
            else
                let fileKind = filepathParam |> CvBase.getCvName
                let elementFullpath = 
                    CvParam.tryGetAttribute "Pathname" filepathParam 
                    |> Option.get 
                    |> Param.getValueAsString 
                    |> String.replace "/" "\\"
                let subFolder =
                    match fileKind with
                    | "Protocol REF" -> "protocols"
                    | "Data" ->
                        if elementFullpath |> String.contains "\\assays\\" then "dataset"
                        elif elementFullpath |> String.contains "\\studies\\" then "resources"
                        else ""     // empty string is not an elegant way though `Path.Combine` ignores it
                    | _ -> ""
                let efpTrunc =
                    let i = String.findIndexBack '\\' elementFullpath
                    elementFullpath[.. i]
                Path.Combine(efpTrunc, subFolder, relFilepath)
        if File.Exists fullpath then Success
        else Error (ErrorMessage.XlsxFile.createFromCvParam filepathParam)