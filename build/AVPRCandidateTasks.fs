module AVPRCandidateTasks

open BlackFox.Fake
open System
open System.IO

open Helpers
open ProjectInfo

let private candidateDirectory () =
    match Environment.GetEnvironmentVariable "AVPR_CANDIDATE_PACKAGE_DIR" with
    | value when String.IsNullOrWhiteSpace value ->
        failwith "Set AVPR_CANDIDATE_PACKAGE_DIR to an explicit AVPR candidate artifact directory."
    | value ->
        let directory = Path.GetFullPath value

        if not (Directory.Exists directory) then
            failwithf "AVPR candidate artifact directory does not exist: %s" directory

        directory

let private nugetPackageVersion directory packageId =
    let prefix = packageId + "."
    let suffix = ".nupkg"

    let candidates =
        Directory.GetFiles(directory, $"{packageId}.*.nupkg")
        |> Array.filter (fun path ->
            let fileName = Path.GetFileName path

            not (path.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase))
            && fileName.Length > prefix.Length
            && Char.IsDigit fileName[prefix.Length]
        )

    match candidates with
    | [| candidate |] ->
        let fileName = Path.GetFileName candidate

        fileName.Substring(
            prefix.Length,
            fileName.Length - prefix.Length - suffix.Length
        )
    | _ ->
        failwithf
            "Expected exactly one %s candidate package in %s, found %i."
            packageId
            directory
            candidates.Length

let private setVersionProperty name version =
    Environment.SetEnvironmentVariable(name, $"[{version}]")

let testAVPRCandidate =
    BuildTask.create "TestAVPRCandidate" [] {
        let directory = candidateDirectory ()
        let clientVersion = nugetPackageVersion directory "AVPRClient"
        let interopVersion = nugetPackageVersion directory "AVPRClient.Interop"
        let modelVersion = nugetPackageVersion directory "ValidationPackage.Model"
        let codecsVersion = nugetPackageVersion directory "ValidationPackage.Codecs"

        setVersionProperty "AVPRClientPackageVersion" clientVersion
        setVersionProperty "AVPRClientInteropPackageVersion" interopVersion
        setVersionProperty "ValidationPackageModelPackageVersion" modelVersion
        setVersionProperty "ValidationPackageCodecsPackageVersion" codecsVersion
        Environment.SetEnvironmentVariable("VALIDATION_PACKAGE_MODEL_NATIVE_VERSION", modelVersion)
        Environment.SetEnvironmentVariable("VALIDATION_PACKAGE_CODECS_NATIVE_VERSION", codecsVersion)
        Environment.SetEnvironmentVariable("AVPR_NATIVE_PACKAGE_DIR", directory)

        let config =
            writeNuGetConfigWithSources
                (Path.Combine(artifactsDir, "avpr-candidate.NuGet.config"))
                [ directory ]

        Environment.SetEnvironmentVariable("RestoreConfigFile", config)

        printfn "Testing explicit AVPR candidate artifacts from %s" directory
        printfn "AVPRClient %s; AVPRClient.Interop %s" clientVersion interopVersion
        printfn "ValidationPackage.Model %s; ValidationPackage.Codecs %s" modelVersion codecsVersion

        let solutionCache =
            Path.Combine(packageCacheDir, "avpr-candidate-solution")
            |> Path.GetFullPath

        recreateDirectory solutionCache

        runDotNetCommand
            "restore"
            $"""{solutionFile} --configfile "{config}" --packages "{solutionCache}" --no-cache"""
            "."

        runDotNetCommand
            "build"
            $"{solutionFile} --configuration Release --no-restore -m:1 -tl:off"
            "."

        let smokeProject =
            "tests/AVPR.CandidatePackageSmoke/AVPR.CandidatePackageSmoke.fsproj"

        runDotNetCommand
            "restore"
            $"""{smokeProject} --configfile "{config}" --packages "{solutionCache}" --no-cache"""
            "."

        runDotNetCommand
            "run"
            $"--project {smokeProject} --configuration Release --no-restore"
            "."

        let buildAssembly =
            System.Reflection.Assembly.GetExecutingAssembly().Location

        runDotNetCommand
            buildAssembly
            "TestPortableARCExpect"
            "."
    }