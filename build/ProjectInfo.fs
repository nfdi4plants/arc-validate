module ProjectInfo

open Fake.Core
open System
open System.Globalization
open System.IO
open System.Text.RegularExpressions
open System.Xml.Linq


/// Contains relevant information about a project (e.g. version info, project location)
type ProjectInfo = {
    Name: string
    ProjFile: string
    AssemblyVersion: string
    AssemblyInformationalVersion: string
} with 
    /// creates a ProjectInfo given a name, project file path, and release notes file path.
    /// version info is created from the version header of the uppermost release notes entry.
    /// Assembly version is set to X.0.0, where X is the major version from the releas enotes.
    static member create(
        name: string,
        projFile: string,
        releaseNotesPath: string
    ): ProjectInfo = 
        let release = releaseNotesPath |> ReleaseNotes.load
        let stableVersion = release.NugetVersion |> SemVer.parse
        let stableVersionTag = $"{stableVersion.Major}.{stableVersion.Minor}.{stableVersion.Patch}"
        let assemblyVersion = $"{stableVersion.Major}.0.0"
        let assemblyInformationalVersion = stableVersionTag
        {
            Name = name
            ProjFile = projFile
            AssemblyVersion = assemblyVersion
            AssemblyInformationalVersion = assemblyInformationalVersion
        }    
    static member create(
        name: string,
        projFile: string
    ): ProjectInfo = 
        {
            Name = name
            ProjFile = projFile
            AssemblyVersion = ""
            AssemblyInformationalVersion = ""
        }

// adapt this to reflect the core project in your repository. The only effect this will have is the version displayed in the docs, as it is currently only possible to have one version displayed there.
let CoreProject = ProjectInfo.create("ARCExpect", "src/ARCExpect/ARCExpect.fsproj", "src/ARCExpect/RELEASE_NOTES.md")
let ARCExpectContractTestsProject = "tests/ARCExpect.Contract.Tests/ARCExpect.Contract.Tests.fsproj"
let ARCExpectJavaScriptProject = "src/ARCExpect/ARCExpect.Javascript.fsproj"
let ARCExpectPythonProject = "src/ARCExpect/ARCExpect.Python.fsproj"
let ARCExpectJavaScriptTestsProject = "tests/ARCExpect.Javascript.Tests/ARCExpect.Javascript.Tests.fsproj"
let ARCExpectPythonTestsProject = "tests/ARCExpect.Python.Tests/ARCExpect.Python.Tests.fsproj"
let ARCExpectPackageSmokeProject = "tests/ARCExpect.PackageSmoke/ARCExpect.PackageSmoke.fsproj"

let private packageVersionProperty name =
    let document = XDocument.Load("Directory.Build.props")

    document.Descendants(XName.Get(name))
    |> Seq.exactlyOne
    |> fun element -> element.Value.Trim().Trim([| '['; ']' |])

let ARCExpectPackageVersion =
    packageVersionProperty "ARCExpectPackageVersion"

let private versionHeadingCandidate =
    Regex(@"^#{1,6}\s+\[?v?\d+\.\d+\.\d+", RegexOptions.Compiled)

let private canonicalReleaseHeading =
    Regex(
        @"^## (?<version>\d+\.\d+\.\d+(?:-[0-9A-Za-z.-]+)?(?:\+[0-9A-Za-z.-]+)?) - (?<date>\d{4}-\d{2}-\d{2})$",
        RegexOptions.Compiled
    )

let validateReleaseMetadata () =
    let heading =
        File.ReadLines("src/ARCExpect/RELEASE_NOTES.md")
        |> Seq.tryFind versionHeadingCandidate.IsMatch
        |> Option.defaultWith (fun () ->
            failwith "ARCExpect has no version heading in src/ARCExpect/RELEASE_NOTES.md.")

    let matched = canonicalReleaseHeading.Match heading

    if not matched.Success then
        failwithf
            "ARCExpect latest release heading must use '## <version> - YYYY-MM-DD'. Found: %s"
            heading

    let date = matched.Groups["date"].Value
    let mutable parsedDate = DateTime.MinValue

    if
        not (
            DateTime.TryParseExact(
                date,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                &parsedDate
            )
        )
    then
        failwithf "ARCExpect latest release heading contains an invalid date: %s" date

    let releaseNotesVersion =
        matched.Groups["version"].Value

    if ARCExpectPackageVersion <> releaseNotesVersion then
        failwithf
            "ARCExpectPackageVersion '%s' does not match the latest release-notes version '%s'."
            ARCExpectPackageVersion
            releaseNotesVersion

    printfn "ARCExpect release metadata: %s" ARCExpectPackageVersion

let ValidationPackageModelNativeVersion =
    packageVersionProperty "ValidationPackageModelPackageVersion"

let ValidationPackageCodecsNativeVersion =
    packageVersionProperty "ValidationPackageCodecsPackageVersion"

let toPythonPackageVersion (version: string) =
    version
        .Replace("-alpha.", "a")
        .Replace("-beta.", "b")
        .Replace("-preview.", "a")
        .Replace("-rc.", "rc")

let ValidationPackageModelPythonVersion =
    toPythonPackageVersion ValidationPackageModelNativeVersion

let ValidationPackageCodecsPythonVersion =
    toPythonPackageVersion ValidationPackageCodecsNativeVersion
let CLIProject = ProjectInfo.create("arc-validate", "src/arc-validate/arc-validate.fsproj", "src/arc-validate/RELEASE_NOTES.md")

let projects = 
    [
        // add relative paths (from project root) to your projects here, including individual reslease notes files
        // e.g. ProjectInfo.create("MyProject", "src/MyProject/MyProject.fsproj", "src/MyProject/RELEASE_NOTES.md")
        CoreProject
        CLIProject
    ]



let testProjects = 
    [
        // add relative paths (from project root) to your testprojects here
        // e.g. ProjectInfo.create("MyTestProject", "tests/MyTestProject/MyTestProject.fsproj")
        ProjectInfo.create("ARCExpect.Tests", "tests/ARCExpect.Tests/ARCExpect.Tests.fsproj")
        ProjectInfo.create("arc-validate.PackageManagement.Tests", "tests/arc-validate.Tests/PackageManagement/arc-validate.PackageManagement.Tests.fsproj")
        ProjectInfo.create("arc-validate.Tests", "tests/arc-validate.Tests/arc-validate.Tests.fsproj")
    ]

let integrationTestProjects =
    [
        ProjectInfo.create(
            "arc-validate.PackageManagement.IntegrationTests",
            "tests/arc-validate.Tests/PackageManagement.Integration/arc-validate.PackageManagement.IntegrationTests.fsproj"
        )
        ProjectInfo.create(
            "arc-validate.IntegrationTests",
            "tests/arc-validate.Tests/Integration/arc-validate.IntegrationTests.fsproj"
        )
    ]


let project = "arc-validate"

let solutionFile  = $"{project}.slnx"

let configuration = "Release"

let gitOwner = "nfdi4plants"

let gitHome = $"https://github.com/{gitOwner}"

let projectRepo = $"https://github.com/{gitOwner}/{project}"

let artifactsDir = "artifacts"
let portableArtifactsDir = System.IO.Path.Combine(artifactsDir, "portable")
let packageDir = System.IO.Path.Combine(artifactsDir, "packages")
let packageCacheDir = System.IO.Path.Combine(artifactsDir, "package-cache")


/// docs are always targeting the version of the core project
let stableDocsVersionTag = ARCExpectPackageVersion
