namespace Validate

open ArcGraphModel
open FSharpAux
open System.IO
open OntologyHelperFunctions


/// Functions to validate #IParam entities.
module Param =

    /// Validates a filepath.
    let filepath<'T when 'T :> IParam> (filepathParam : 'T) =
        let relFilepath = Param.getValueAsString filepathParam
        let fullpath =
            // if relative path from ARC root is provided
            if String.contains  "/" relFilepath || String.contains "\\" relFilepath then
                Path.Combine(ArcPaths.inputPath, relFilepath)
            // if only filename is provided, storage in dataset folder is assumed
            else
                let sfp = CvParam.tryGetAttribute "Pathname" filepathParam |> Option.get |> Param.getValueAsString |> String.replace "/" "\\"
                let sfpTrunc =
                    let i = String.findIndexBack '\\' sfp
                    sfp[.. i]
                Path.Combine(sfpTrunc, "dataset", relFilepath)
        if File.Exists fullpath then Success
        else Error (ErrorMessage.XlsxFile.createFromCvParam filepathParam)