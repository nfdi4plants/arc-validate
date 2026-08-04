module DocsSampleTasks

open BlackFox.Fake
open System
open System.IO

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
    arcDirectory, outputDirectory

let private verifyOutputFiles topic outputDirectory =
    for fileName in outputFileNames do
        let path = Path.Combine(outputDirectory, fileName)

        if not (File.Exists path) || FileInfo(path).Length = 0L then
            failwithf "%s did not generate a non-empty %s" topic fileName

    let summary = File.ReadAllText(Path.Combine(outputDirectory, "validation_summary.json"))
    let junit = File.ReadAllText(Path.Combine(outputDirectory, "validation_report.xml"))
    let badge = File.ReadAllText(Path.Combine(outputDirectory, "badge.svg"))

    if not (summary.Contains "\"ValidationPackage\"") then
        failwithf "%s generated an invalid validation summary" topic

    if not (junit.Contains "<testsuites>") then
        failwithf "%s generated an invalid JUnit report" topic

    if not (badge.Contains "<svg") then
        failwithf "%s generated an invalid badge" topic

let private publishOutputFiles topic outputDirectory =
    let destination = Path.Combine(samplesSourceDirectory, topic)

    for fileName in outputFileNames do
        File.Copy(
            Path.Combine(outputDirectory, fileName),
            Path.Combine(destination, fileName),
            true
        )

let private verifyPortableOutputs topic outputDirectory =
    for fileName in [| "validation_summary.json"; "badge.svg" |] do
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

    let localSource = Uri(Path.GetFullPath packageDir).AbsoluteUri
    let packageReference =
        $"#i \"nuget: {localSource}\"{Environment.NewLine}{fsxReferenceLine}"

    source.Replace(fsxReferenceLine, packageReference)

let runDocsSamplesDotNet =
    BuildTask.create "RunDocsSamplesDotNet" [ packARCExpect ] {
        let directory = Path.Combine(samplesScratchDirectory, "dotnet")
        recreateDirectory directory

        for sample in samplesByExtension ".fsx" do
            let topic = topicOf sample
            let script = topic + ".fsx"
            let arcDirectory, outputDirectory =
                prepareOutputDirectories directory topic

            File.WriteAllText(Path.Combine(directory, script), pinnedFsx sample)
            runCommand
                "dotnet"
                ([ "fsi"; script ]
                 @ argumentsForTopic topic arcDirectory outputDirectory)
                directory
            verifyOutputFiles topic outputDirectory
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

        runUv [ "pip"; "install"; "--python"; python; wheel ] "."

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
            verifyPortableOutputs topic outputDirectory
    }

let runDocsSamples =
    BuildTask.createEmpty
        "RunDocsSamples"
        [ runDocsSamplesPython ]
