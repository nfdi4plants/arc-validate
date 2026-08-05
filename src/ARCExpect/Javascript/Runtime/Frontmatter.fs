namespace ARCExpect

[<RequireQualifiedAccess>]
module internal TargetFrontmatter =

    let metadata (_frontmatter: string) =
        invalidOp
            "Setup.Metadata is unavailable for JavaScript until AVPR defines a JavaScript validation-package frontmatter format."
