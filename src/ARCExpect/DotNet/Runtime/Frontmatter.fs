namespace ARCExpect

open ValidationPackage.Codecs

[<RequireQualifiedAccess>]
module internal TargetFrontmatter =

    let metadata frontmatter =
        ValidationPackageYaml.extractOrFail
            FrontmatterLanguage.FSharp
            frontmatter
