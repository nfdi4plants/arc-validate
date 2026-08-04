namespace ARCExpect

open Fable.Core
open Fable.Pyxpecto.Model
open ValidationPackage.Codecs
open ValidationPackage.Model


type FrontmatterLanguage =
    | FSharpFrontmatter
    | PythonFrontmatter

[<RequireQualifiedAccess>]
module private FrontmatterLanguageConversion =

    let toCodec language =
        match language with
        | FSharpFrontmatter -> ValidationPackage.Codecs.FrontmatterLanguage.FSharp
        | PythonFrontmatter -> ValidationPackage.Codecs.FrontmatterLanguage.Python

[<RequireQualifiedAccess>]
module private RuntimeFrontmatter =

    let normalize language (frontmatter: string) =
        if isNull frontmatter then
            frontmatter
        else
            match language with
            | PythonFrontmatter ->
                let normalized =
                    frontmatter.Replace("\r\n", "\n").Replace("\r", "\n")

                if
                    normalized.StartsWith("\n---\n", System.StringComparison.Ordinal)
                    && normalized.EndsWith("---\n", System.StringComparison.Ordinal)
                then
                    "\"\"\"" + normalized + "\"\"\""
                else
                    frontmatter
            | FSharpFrontmatter -> frontmatter

[<AttachMembers>]
type Setup =

    static member Metadata(
        frontmatter: string,
        programmingLanguage: FrontmatterLanguage
    ) =
        ValidationPackageYaml.extractOrFail
            (FrontmatterLanguageConversion.toCodec programmingLanguage)
            (RuntimeFrontmatter.normalize programmingLanguage frontmatter)

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

    static member ValidationPackageFromValues(
        name: string,
        summary: string,
        description: string,
        majorVersion: int,
        minorVersion: int,
        patchVersion: int,
        programmingLanguage: string,
        ?PreReleaseVersionSuffix: string,
        ?BuildMetadataVersionSuffix: string,
        ?Publish: bool,
        ?Authors: Author array,
        ?Tags: OntologyAnnotation array,
        ?ReleaseNotes: string,
        ?CQCHookEndpoint: string,
        ?Inputs: CommandInputParameter array,
        ?CriticalValidationCases: TestCase array,
        ?NonCriticalValidationCases: TestCase array
    ) =
        Setup.ValidationPackage(
            ValidationPackageMetadata.create(
                name,
                summary,
                description,
                majorVersion,
                minorVersion,
                patchVersion,
                programmingLanguage,
                ?PreReleaseVersionSuffix = PreReleaseVersionSuffix,
                ?BuildMetadataVersionSuffix = BuildMetadataVersionSuffix,
                ?Publish = Publish,
                ?Authors = Authors,
                ?Tags = Tags,
                ?ReleaseNotes = ReleaseNotes,
                ?CQCHookEndpoint = CQCHookEndpoint,
                ?Inputs = Inputs
            ),
            ?CriticalValidationCases = CriticalValidationCases,
            ?NonCriticalValidationCases = NonCriticalValidationCases
        )
