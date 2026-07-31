namespace ARCExpect

open ARCExpect.Badge
open ARCExpect.JUnit
open Expecto
open System.IO
open ValidationPackage.Codecs
open ValidationPackage.Model

type FrontmatterLanguage =
    | FSharpFrontmatter
    | PythonFrontmatter

[<RequireQualifiedAccess>]
module private FrontmatterLanguage =

    let toCodec language =
        match language with
        | FSharpFrontmatter -> ValidationPackage.Codecs.FrontmatterLanguage.FSharp
        | PythonFrontmatter -> ValidationPackage.Codecs.FrontmatterLanguage.Python

type Setup =
    
    static member Metadata(
        frontmatter: string,
        programmingLanguage: FrontmatterLanguage
    ) =
        ValidationPackageYaml.extractOrFail
            (FrontmatterLanguage.toCodec programmingLanguage)
            frontmatter

    static member Metadata(
        frontmatter: string,
        programmingLanguage: ValidationPackage.Codecs.FrontmatterLanguage
    ) =
        ValidationPackageYaml.extractOrFail programmingLanguage frontmatter

    static member ValidationPackage(
        metadata: ValidationPackageMetadata,
        ?CriticalValidationCases: Test list,
        ?NonCriticalValidationCases: Test list
    ) =
        ARCValidationPackage.create(
            metadata = metadata,
            ?CriticalValidationCasesList = CriticalValidationCases,
            ?NonCriticalValidationCasesList = NonCriticalValidationCases
        )

    static member ValidationPackage(
        name: string,
        summary: string,
        description: string,
        majorVersion: int,
        minorVersion: int,
        patchVersion: int,
        programmingLanguage: string,
        ?Publish: bool,
        ?Authors: Author array,
        ?Tags: OntologyAnnotation array,
        ?ReleaseNotes: string,
        ?CriticalValidationCases: Test list,
        ?NonCriticalValidationCases: Test list,
        ?CQCHookEndpoint: string
    ) =
        Setup.ValidationPackage(
            metadata = ValidationPackageMetadata.create(
                name = name,
                summary = summary,
                description = description,
                majorVersion = majorVersion,
                minorVersion = minorVersion,
                patchVersion = patchVersion,
                programmingLanguage = programmingLanguage,
                ?Publish = Publish,
                ?Authors = Authors,
                ?Tags = Tags,
                ?ReleaseNotes = ReleaseNotes,
                ?CQCHookEndpoint = CQCHookEndpoint
            ),
            ?CriticalValidationCases = CriticalValidationCases,
            ?NonCriticalValidationCases = NonCriticalValidationCases
        )

open System.Collections.Generic
type Execute =

// ------------------ New API with ARCValidationPackage, metadata support and custom summaries ------------------
    
    static member Validation (
        ?Payload: Dictionary<string, obj>
    ) =
        fun (arcValidationPackage: ARCValidationPackage) ->

            let criticalResults = performTest arcValidationPackage.CriticalValidationCases
            let nonCriticalResults = performTest arcValidationPackage.NonCriticalValidationCases
        
            ValidationSummary.ofExpectoTestRunSummaries(
                criticalSummary = criticalResults,
                nonCriticalSummary = nonCriticalResults,
                package = ValidationPackageSummary.fromMetadata(arcValidationPackage.Metadata),
                ?Payload = (Payload |> Option.map PayloadConversion.fromDictionary)
            )

    static member SummaryCreation(
        path: string
    ) =  
        fun (validationSummary: ValidationSummary) -> 
            File.WriteAllText(path, ValidationSummary.toJson validationSummary)

    static member JUnitReportCreation(
        path: string,
        ?Verbose: bool
    ) =
        let verbose = defaultArg Verbose false

        fun (validationSummary: ValidationSummary) ->
            RunSummary.combine [|
                validationSummary.Critical
                validationSummary.NonCritical
            |]
            |> fun summary -> ARCExpect.JUnit.Writer.toXml(summary, Verbose = verbose)
            |> fun content -> File.WriteAllText(path, content)

    static member BadgeCreation(
        path: string,
        labelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color
    ) =
        fun (validationSummary: ValidationSummary) ->
            let portableThresholds =
                Thresholds
                |> Option.map (fun thresholds ->
                    thresholds
                    |> Map.toArray
                    |> Array.map (fun (value, color) ->
                        Threshold.create(value, color)
                    )
                )

            validationSummary
            |> fun summary ->
                ARCExpect.Badge.Writer.toSvg(
                    summary,
                    labelText,
                    ?ValueSuffix = ValueSuffix,
                    ?Thresholds = portableThresholds,
                    ?DefaultColor = DefaultColor
                )
            |> fun content -> File.WriteAllText(path, content)

    static member ValidationPipeline(
        basePath: string,
        ?BadgeLabelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color,
        ?Payload: Dictionary<string, obj>
    ) =
        fun (arcValidationPackage: ARCValidationPackage) ->

            let labelText = defaultArg BadgeLabelText $"{arcValidationPackage.Metadata.Name}@{ValidationPackageMetadata.getSemanticVersionString arcValidationPackage.Metadata}"

            let foldername = $"{arcValidationPackage.Metadata.Name}@{ValidationPackageMetadata.getSemanticVersionString arcValidationPackage.Metadata}"

            let resultFolder = Path.Combine(basePath, ".arc-validate-results", foldername)
            let summaryPath = Path.Combine(resultFolder, "validation_summary.json")
            let badgePath = Path.Combine(resultFolder, "badge.svg")
            let jUnitPath = Path.Combine(resultFolder, "validation_report.xml")

            Directory.CreateDirectory(resultFolder) |> ignore

            let results = 
                arcValidationPackage
                |> Execute.Validation(?Payload = Payload)

            results |> Execute.SummaryCreation(summaryPath)
            results |> Execute.JUnitReportCreation(jUnitPath)
            results
            |> Execute.BadgeCreation(
                badgePath, 
                labelText, 
                ?ValueSuffix = ValueSuffix, 
                ?Thresholds = Thresholds, 
                ?DefaultColor = DefaultColor
            )

// ------------------ Legacy API without ARCValidationPackage, metadata, or custom Summaries ------------------

    //static member Validation (validationCases: Test) = performTest validationCases

    //static member JUnitSummaryCreation(
    //    path: string,
    //    ?Verbose: bool
    //) =
    //    let verbose = defaultArg Verbose false
    //    fun (validationResults: Impl.TestRunSummary) -> writeJUnitSummary verbose path validationResults

    //static member BadgeCreation(
    //    path: string,
    //    labelText: string,
    //    ?ValueSuffix: string,
    //    ?Thresholds: Map<int, Color>,
    //    ?DefaultColor: Color
    //) =
    //    fun (validationResults: Impl.TestRunSummary) -> 
    //        validationResults
    //        |> BadgeCreation.ofTestResults(
    //            labelText,
    //            ?ValueSuffix = ValueSuffix,
    //            ?Thresholds = Thresholds,
    //            ?DefaultColor = DefaultColor
    //        )
    //        |> fun b -> b.WriteBadge(path)

    //static member ValidationPipeline(
    //    jUnitPath: string,
    //    badgePath: string,
    //    labelText: string,
    //    ?ValueSuffix: string,
    //    ?Thresholds: Map<int, Color>,
    //    ?DefaultColor: Color
    //) =
    //    fun (validationCases: Test) ->

    //        let results = 
    //            validationCases
    //            |> Execute.Validation

    //        results
    //        |> Execute.JUnitSummaryCreation(jUnitPath)

    //        results
    //        |> Execute.BadgeCreation(badgePath, labelText, ?ValueSuffix = ValueSuffix, ?Thresholds = Thresholds, ?DefaultColor = DefaultColor)

    //static member ValidationPipeline(
    //    basePath: string,
    //    packageName: string,
    //    ?BadgeLabelText: string,
    //    ?ValueSuffix: string,
    //    ?Thresholds: Map<int, Color>,
    //    ?DefaultColor: Color
    //) =
    //    fun (validationCases: Test) ->

    //        let resultFolder = Path.Combine(basePath, ".arc-validate-results", packageName)
    //        let badgePath = Path.Combine(resultFolder, "badge.svg")
    //        let jUnitPath = Path.Combine(resultFolder, "validation_report.xml")

    //        Directory.CreateDirectory(resultFolder) |> ignore

    //        let results = 
    //            validationCases
    //            |> Execute.Validation

    //        results
    //        |> Execute.JUnitSummaryCreation(jUnitPath)

    //        let labelText = defaultArg BadgeLabelText packageName

    //        results
    //        |> Execute.BadgeCreation(badgePath, labelText, ?ValueSuffix = ValueSuffix, ?Thresholds = Thresholds, ?DefaultColor = DefaultColor)

