module DocsSampleTasks

open BlackFox.Fake
open System
open System.IO
open System.Text
open System.Text.Json
open System.Xml
open System.Xml.Linq

open Helpers
open PackageTasks
open ProjectInfo

let private samplesSourceDirectory = Path.Combine("docs", "samples")
let private samplesScratchDirectory = Path.Combine(artifactsDir, "docs-samples")
let private outputFileNames =
    [|
        "validation_summary.json"
        "validation_report.xml"
        "badge.svg"
    |]

let private packageIdentityForTopic topic =
    match topic with
    | "command-line-arguments" -> "configurable-validation@1.0.0"
    | "payload" -> "payload-validation@1.0.0"
    | "simple-validation-package" -> "simple-validation@1.0.0"
    | _ -> failwithf "Unknown documentation sample topic: %s" topic

let private resultDirectory topic outputDirectory =
    Path.Combine(
        outputDirectory,
        ".arc-validate-results",
        packageIdentityForTopic topic
    )

let private normalizedSource path =
    File.ReadAllText(path).Replace("\r\n", "\n").Replace("\r", "\n")

let private samplesByExtension extension =
    if Directory.Exists samplesSourceDirectory then
        Directory.GetFiles(
            samplesSourceDirectory,
            "sample" + extension,
            SearchOption.AllDirectories
        )
        |> Array.sort
    else
        Array.empty

let private topicOf (samplePath: string) =
    samplePath |> Path.GetDirectoryName |> Path.GetFileName

let private argumentsForTopic topic arcDirectory outputDirectory =
    let standardArguments = [
        "-i"
        arcDirectory
        "-o"
        outputDirectory
        "--source-branch"
        "docs/polyglot"
        "--source-commit-hash"
        "0123456789abcdef"
    ]

    let customArguments =
        if topic = "command-line-arguments" then
            [
                "--strict"
                "--minimum-files"
                "2"
                "--label"
                "documentation sample"
            ]
        else
            []

    standardArguments @ customArguments

let private prepareOutputDirectories runtimeDirectory topic =
    let topicDirectory = Path.Combine(runtimeDirectory, topic)
    let arcDirectory = Path.Combine(topicDirectory, "arc") |> Path.GetFullPath
    let outputDirectory = Path.Combine(topicDirectory, "output") |> Path.GetFullPath
    Directory.CreateDirectory(arcDirectory) |> ignore
    recreateDirectory outputDirectory

    if topic = "payload" then
        let studyDirectory = Path.Combine(arcDirectory, "studies")
        Directory.CreateDirectory(studyDirectory) |> ignore
        File.WriteAllText(
            Path.Combine(arcDirectory, "investigation.xlsx"),
            "documentation investigation"
        )
        File.WriteAllText(
            Path.Combine(studyDirectory, "study.xlsx"),
            "documentation study"
        )

    arcDirectory, outputDirectory

let private requiredJsonProperty topic (name: string) (element: JsonElement) =
    let mutable value = Unchecked.defaultof<JsonElement>

    if element.TryGetProperty(name, &value) then
        value
    else
        failwithf "%s summary payload is missing %s" topic name

let private verifyTopicPayload topic summaryPath =
    use document = JsonDocument.Parse(File.ReadAllText(summaryPath))
    let root = document.RootElement
    let mutable payload = Unchecked.defaultof<JsonElement>
    let hasPayload = root.TryGetProperty("Payload", &payload)

    match topic with
    | "simple-validation-package" ->
        if hasPayload then
            failwith "simple-validation-package must not generate a payload"
    | "payload" ->
        if not hasPayload then
            failwith "payload sample did not generate a payload"

        let filesChecked =
            payload
            |> requiredJsonProperty topic "Metrics"
            |> requiredJsonProperty topic "FilesChecked"
            |> fun value -> value.GetInt32()

        if filesChecked <> 2 then
            failwithf "payload sample checked %i files instead of 2" filesChecked
    | "command-line-arguments" ->
        if not hasPayload then
            failwith "command-line-arguments sample did not generate a payload"

        let strict =
            payload
            |> requiredJsonProperty topic "Strict"
            |> fun value -> value.GetBoolean()
        let minimumFiles =
            payload
            |> requiredJsonProperty topic "MinimumFiles"
            |> fun value -> value.GetInt32()
        let label =
            payload
            |> requiredJsonProperty topic "Label"
            |> fun value -> value.GetString()

        if not strict || minimumFiles <> 2 || label <> "documentation sample" then
            failwithf
                "command-line-arguments payload did not preserve the supplied values: strict=%b minimum-files=%i label=%s"
                strict
                minimumFiles
                label
    | _ -> failwithf "Unknown documentation sample topic: %s" topic

let private formatSummaryJson (path: string) =
    use document = JsonDocument.Parse(File.ReadAllText(path))
    let options = JsonSerializerOptions(WriteIndented = true)
    let formatted =
        JsonSerializer.Serialize(document.RootElement, options)
            .ReplaceLineEndings("\n")

    File.WriteAllText(path, formatted + "\n", UTF8Encoding(false))

let private formatJUnitXml (path: string) =
    let document = XDocument.Load(path)

    document.Descendants()
    |> Seq.filter (fun (element: XElement) ->
        element.Name.LocalName = "testcase"
        || element.Name.LocalName = "testsuite"
    )
    |> Seq.iter (fun (element: XElement) ->
        element.SetAttributeValue(XName.Get("time"), "0.000")
    )

    let settings =
        XmlWriterSettings(
            Encoding = UTF8Encoding(false),
            Indent = true,
            IndentChars = "  ",
            NewLineChars = "\n",
            NewLineHandling = NewLineHandling.None,
            OmitXmlDeclaration = false
        )

    use writer = XmlWriter.Create(path, settings)
    document.Save(writer)

let private formatOutputFiles topic outputDirectory =
    let outputDirectory = resultDirectory topic outputDirectory
    formatSummaryJson(Path.Combine(outputDirectory, "validation_summary.json"))
    formatJUnitXml(Path.Combine(outputDirectory, "validation_report.xml"))

let private verifyOutputFiles topic outputDirectory =
    let outputDirectory = resultDirectory topic outputDirectory

    for fileName in outputFileNames do
        let path = Path.Combine(outputDirectory, fileName)

        if not (File.Exists path) || FileInfo(path).Length = 0L then
            failwithf "%s did not generate a non-empty %s" topic fileName

    let summaryPath = Path.Combine(outputDirectory, "validation_summary.json")
    let summary = File.ReadAllText(summaryPath)
    let junit = File.ReadAllText(Path.Combine(outputDirectory, "validation_report.xml"))
    let badge = File.ReadAllText(Path.Combine(outputDirectory, "badge.svg"))

    if not (summary.Contains "\"ValidationPackage\"") then
        failwithf "%s generated an invalid validation summary" topic

    if not (junit.Contains "<testsuites>") then
        failwithf "%s generated an invalid JUnit report" topic

    if not (badge.Contains "<svg") then
        failwithf "%s generated an invalid badge" topic

    verifyTopicPayload topic summaryPath

let private publishOutputFiles topic outputDirectory =
    let outputDirectory = resultDirectory topic outputDirectory
    let destination = Path.Combine(samplesSourceDirectory, topic)

    for fileName in outputFileNames do
        File.Copy(
            Path.Combine(outputDirectory, fileName),
            Path.Combine(destination, fileName),
            true
        )

let private verifyPortableOutputs topic outputDirectory =
    let outputDirectory = resultDirectory topic outputDirectory

    for fileName in outputFileNames do
        let expected = normalizedSource(Path.Combine(samplesSourceDirectory, topic, fileName))

        let actual = normalizedSource(Path.Combine(outputDirectory, fileName))

        if actual <> expected then
            failwithf "Python %s output differs from the F#/.NET output for %s" fileName topic

let private fsxReferenceLine =
    $"#r \"nuget: ARCExpect, {ARCExpectPackageVersion}\""
let private fsharpFrontmatterStart =
    "let [<Literal>]PACKAGE_METADATA = \"\"\"(*\n---"

let private pinnedFsx samplePath =
    let source = normalizedSource samplePath

    if not (source.StartsWith(fsharpFrontmatterStart, StringComparison.Ordinal)) then
        failwithf "%s must start immediately with F# PACKAGE_METADATA frontmatter" samplePath

    if not (source.Contains fsxReferenceLine) then
        failwithf "%s must contain %s" samplePath fsxReferenceLine

    let packageReference =
        let arcExpectPackage =
            Path.Combine(packageDir, $"ARCExpect.{ARCExpectPackageVersion}.nupkg")
        let packageFingerprint = fileSha256 arcExpectPackage
        let packageSource =
            prepareFingerprintedNuGetSource
                (Path.Combine(samplesScratchDirectory, "nuget"))
                arcExpectPackage
                (packageDir :: (localNativeDependencyPackageDirectory () |> Option.toList))
            |> Path.GetFullPath
            |> Uri

        String.concat
            Environment.NewLine
            [
                $"#i \"nuget: {packageSource.AbsoluteUri}\""
                fsxReferenceLine
                $"// Package fingerprint: {packageFingerprint}"
            ]

    source.Replace(fsxReferenceLine, packageReference)

let runDocsSamplesDotNet =
    BuildTask.create "RunDocsSamplesDotNet" [ packARCExpect ] {
        let directory = Path.Combine(samplesScratchDirectory, "dotnet")
        let cacheDirectory = Path.Combine(packageCacheDir, "docs-samples") |> Path.GetFullPath
        recreateDirectory directory
        recreateDirectory cacheDirectory

        for sample in samplesByExtension ".fsx" do
            let topic = topicOf sample
            let script = topic + ".fsx"
            let arcDirectory, outputDirectory =
                prepareOutputDirectories directory topic

            File.WriteAllText(Path.Combine(directory, script), pinnedFsx sample)
            runCommandWithEnvironmentVariable
                "dotnet"
                ([ "fsi"; script ]
                 @ argumentsForTopic topic arcDirectory outputDirectory)
                directory
                "NUGET_PACKAGES"
                cacheDirectory
            verifyOutputFiles topic outputDirectory
            formatOutputFiles topic outputDirectory
            publishOutputFiles topic outputDirectory
    }

let runDocsSamplesPython =
    BuildTask.create "RunDocsSamplesPython" [ runDocsSamplesDotNet ] {
        let directory = Path.Combine(samplesScratchDirectory, "python")
        let environmentDirectory = Path.Combine(directory, ".venv")
        recreateDirectory directory

        let wheel =
            Path.Combine(
                packageDir,
                $"arcexpect-{toPythonPackageVersion ARCExpectPackageVersion}-py3-none-any.whl"
            )
            |> Path.GetFullPath

        runUv [ "venv"; environmentDirectory ] "."

        let python =
            if OperatingSystem.IsWindows() then
                Path.Combine(environmentDirectory, "Scripts", "python.exe")
            else
                Path.Combine(environmentDirectory, "bin", "python")

        let dependencyWheels =
            match localNativeDependencyPackageDirectory () with
            | Some directory ->
                [
                    localNativeDependencyArtifact
                        directory
                        $"validationpackage_model-{ValidationPackageModelPythonVersion}-py3-none-any.whl"
                    localNativeDependencyArtifact
                        directory
                        $"validationpackage_codecs-{ValidationPackageCodecsPythonVersion}-py3-none-any.whl"
                ]
            | None -> []

        runUv
            ([ "pip"; "install"; "--python"; python ]
             @ dependencyWheels
             @ [ wheel ])
            "."

        for sample in samplesByExtension ".py" do
            let source = normalizedSource sample

            if
                not (
                    source.StartsWith(
                        "PACKAGE_METADATA = \"\"\"\n---",
                        StringComparison.Ordinal
                    )
                )
            then
                failwithf "%s must start immediately with Python PACKAGE_METADATA frontmatter" sample

            let dependency =
                $"#   \"arcexpect=={toPythonPackageVersion ARCExpectPackageVersion}\","

            if not (source.Contains dependency) then
                failwithf "%s must declare the current ARCExpect wheel in PEP 723 metadata" sample

            let topic = topicOf sample
            let script = topic + ".py"
            let arcDirectory, outputDirectory =
                prepareOutputDirectories directory topic

            File.Copy(sample, Path.Combine(directory, script), true)
            runCommand
                python
                (script :: argumentsForTopic topic arcDirectory outputDirectory)
                directory
            verifyOutputFiles topic outputDirectory
            formatOutputFiles topic outputDirectory
            verifyPortableOutputs topic outputDirectory
    }

let runDocsSamples =
    BuildTask.createEmpty
        "RunDocsSamples"
        [ runDocsSamplesPython ]
