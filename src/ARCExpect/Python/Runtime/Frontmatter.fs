namespace ARCExpect

open System
open ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module internal TargetFrontmatter =

    let private yamlBody (frontmatter: string) =
        if isNull frontmatter then
            nullArg "frontmatter"

        let normalized =
            frontmatter.Replace("\r\n", "\n").Replace("\r", "\n")

        let startMarker = "\n---\n"
        let endMarker = "---\n"

        if
            normalized.StartsWith(startMarker, StringComparison.Ordinal)
            && normalized.EndsWith(endMarker, StringComparison.Ordinal)
        then
            normalized.Substring(
                startMarker.Length,
                normalized.Length - startMarker.Length - endMarker.Length
            )
        else
            invalidArg
                "frontmatter"
                "Setup.Metadata expects the runtime value of Python PACKAGE_METADATA."

    let metadata frontmatter =
        let packageMetadata =
            frontmatter
            |> yamlBody
            |> ValidationPackageYaml.decodeOrFail

        packageMetadata.ProgrammingLanguage <- "Python"
        packageMetadata
