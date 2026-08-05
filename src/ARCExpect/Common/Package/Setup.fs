namespace ARCExpect

open Fable.Core
open Fable.Pyxpecto.Model
open ValidationPackage.Model

[<AttachMembers>]
type Setup =

    static member Metadata(frontmatter: string) =
        TargetFrontmatter.metadata frontmatter

    static member ValidationPackage(
        metadata: ValidationPackageMetadata,
        ?CriticalValidationCases: TestCase array,
        ?NonCriticalValidationCases: TestCase array
    ) =
        ARCValidationPackage.create(
            metadata,
            ?CriticalValidationCases = CriticalValidationCases,
            ?NonCriticalValidationCases = NonCriticalValidationCases
        )
