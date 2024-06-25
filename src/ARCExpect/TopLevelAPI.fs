namespace ARCExpect

open AnyBadge.NET
open Expecto
open System.IO
open AVPRIndex

type Setup =
    
    static member Metadata(
        frontmatter: string
    ) =
        ValidationPackageMetadata.extractFromString frontmatter

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
                ?Publish = Publish,
                ?Authors = Authors,
                ?Tags = Tags,
                ?ReleaseNotes = ReleaseNotes,
                ?CQCHookEndpoint = CQCHookEndpoint
            ),
            ?CriticalValidationCases = CriticalValidationCases,
            ?NonCriticalValidationCases = NonCriticalValidationCases
        )

type Execute =

// ------------------ New API with ARCValidationPackage, metadata support and custom summaries ------------------
    static member Validation (arcValidationPackage: ARCValidationPackage) =
        let criticalResults = performTest arcValidationPackage.CriticalValidationCases
        let nonCriticalResults = performTest arcValidationPackage.NonCriticalValidationCases
        
        ValidationSummary.ofExpectoTestRunSummaries(
            criticalSummary = criticalResults,
            nonCriticalSummary = nonCriticalResults,
            package = ValidationPackageSummary.create(arcValidationPackage.Metadata)
        )

    static member SummaryCreation(
        path: string
    ) =  
        fun (validationSummary: ValidationSummary) -> 
            ValidationSummary.writeJson path validationSummary

    static member JUnitReportCreation(
        path: string,
        ?Verbose: bool
    ) =
        let verbose = defaultArg Verbose false

        fun (validationSummary: ValidationSummary) -> 
            match validationSummary.Critical.OriginalRunSummary, validationSummary.NonCritical.OriginalRunSummary with
            | None, None ->
                printfn "No validation results to summarize"
            | Some criticalResults, None ->
                writeJUnitSummary verbose path criticalResults
            | None, Some nonCriticalResults ->
                writeJUnitSummary verbose path nonCriticalResults
            | Some criticalResults, Some nonCriticalResults ->
                combineTestRunSummaries [criticalResults; nonCriticalResults]
                |> writeJUnitSummary verbose path

    static member BadgeCreation(
        path: string,
        labelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color
    ) =
        fun (validationSummary: ValidationSummary) -> 

            validationSummary
            |> BadgeCreation.ofValidationSummary(
                labelText,
                ?ValueSuffix = ValueSuffix,
                ?Thresholds = Thresholds,
                ?DefaultColor = DefaultColor
            )
            |> fun b -> b.WriteBadge(path)

    static member ValidationPipeline(
        basePath: string,
        ?BadgeLabelText: string,
        ?ValueSuffix: string,
        ?Thresholds: Map<int, Color>,
        ?DefaultColor: Color
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
                |> Execute.Validation

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

