namespace ARCExpect

open System.IO

[<RequireQualifiedAccess>]
module internal OutputFileSystem =

    let write basePath (outputs: PipelineOutputBundle) =
        let resultFolder =
            Path.Combine(
                basePath,
                outputs.ResultDirectoryName,
                outputs.FolderName
            )

        Directory.CreateDirectory(resultFolder) |> ignore
        File.WriteAllText(
            Path.Combine(resultFolder, outputs.SummaryFileName),
            outputs.SummaryJson
        )
        File.WriteAllText(
            Path.Combine(resultFolder, outputs.JUnitFileName),
            outputs.JUnitXml
        )
        File.WriteAllText(
            Path.Combine(resultFolder, outputs.BadgeFileName),
            outputs.BadgeSvg
        )
