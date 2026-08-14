module VersionResolutionTests

open Expecto
open ARCValidate.Configuration
open ValidationPackage.Codecs
open ValidationPackage.Model

let private version value =
    SemVer.tryParse value |> Option.defaultWith (fun () -> failtestf "Invalid test SemVer: %s" value)

let private identity name value =
    ValidationPackageIdentity.create(name, version value)

let private canonical selections =
    selections
    |> ValidationPackagesConfig.create
    |> DecodedValidationPackagesConfig.canonical

let private selection name floor policy =
    ValidationPackageSelection.create(name, version floor, RollForward = policy)

let private resolve decoded identities =
    VersionResolution.resolve decoded identities

let private expectConfigurationFailure action =
    try
        action () |> ignore
        failtest "Expected a configuration failure."
    with
    | ConfigurationException _ -> ()

[<Tests>]
let ``version resolution policy tests`` =
    testList "version resolution" [
        test "disable requires the exact full identity including suffixes" {
            let decoded =
                canonical [|
                    selection "package" "1.2.3-preview.1+build.4" RollForwardPolicy.Disable
                |]

            let actual =
                resolve decoded [|
                    identity "package" "1.2.3"
                    identity "package" "1.2.3-preview.1+build.4"
                |]

            Expect.equal
                (SemVer.toString actual.ValidationPackages[0].ResolvedVersion)
                "1.2.3-preview.1+build.4"
                "Exact identity"
        }

        test "latest_patch accepts an absent floor and chooses the highest stable patch" {
            let decoded = canonical [| selection "package" "1.2.3" RollForwardPolicy.LatestPatch |]

            let actual =
                resolve decoded [|
                    identity "package" "1.2.2"
                    identity "package" "1.2.4"
                    identity "package" "1.2.8"
                    identity "package" "1.2.9-preview.1"
                    identity "package" "1.3.0"
                |]

            Expect.equal
                (SemVer.toString actual.ValidationPackages[0].ResolvedVersion)
                "1.2.8"
                "Patch policy"
        }

        test "latest_minor stays within the major and rejects suffix candidates" {
            let decoded = canonical [| selection "package" "2.1.5" RollForwardPolicy.LatestMinor |]

            let actual =
                resolve decoded [|
                    identity "package" "2.1.5"
                    identity "package" "2.9.0"
                    identity "package" "2.10.0+build.1"
                    identity "package" "3.0.0"
                |]

            Expect.equal
                (SemVer.toString actual.ValidationPackages[0].ResolvedVersion)
                "2.9.0"
                "Minor policy"
        }

        test "rolling policies reject prerelease and build floors" {
            [
                "1.2.3-preview.1", RollForwardPolicy.LatestPatch
                "1.2.3+build.1", RollForwardPolicy.LatestMinor
            ]
            |> List.iter (fun (floor, policy) ->
                expectConfigurationFailure (fun () ->
                    resolve
                        (canonical [| selection "package" floor policy |])
                        [| identity "package" "1.2.4" |]
                )
            )
        }

        test "source selection order is deterministic and independent of index order" {
            let decoded = canonical [|
                selection "second" "1.0.0" RollForwardPolicy.Disable
                selection "first" "2.0.0" RollForwardPolicy.Disable
            |]

            let actual = resolve decoded [| identity "first" "2.0.0"; identity "second" "1.0.0" |]

            Expect.sequenceEqual
                (actual.ValidationPackages |> Array.map _.Name)
                [| "second"; "first" |]
                "Source order"
        }

        test "legacy name-only uses the highest stable version across majors" {
            let legacy =
                LegacyValidationPackagesConfig.create [|
                    LegacyValidationPackageSelection.create("package")
                |]
                |> DecodedValidationPackagesConfig.legacy

            let actual = resolve legacy [|
                identity "package" "1.9.0"
                identity "package" "3.0.0-preview.1"
                identity "package" "2.5.0"
            |]

            let package = actual.ValidationPackages[0]
            Expect.equal package.RequestedVersion None "Legacy requested version"
            Expect.equal package.RollForward ExecutionPlanRollForward.LegacyLatestStable "Legacy policy"
            Expect.equal (SemVer.toString package.ResolvedVersion) "2.5.0" "Highest stable identity"
        }

        test "missing exact and rolling candidates are configuration failures" {
            [
                selection "package" "1.0.0" RollForwardPolicy.Disable
                selection "package" "2.0.0" RollForwardPolicy.LatestPatch
                selection "package" "3.0.0" RollForwardPolicy.LatestMinor
            ]
            |> List.iter (fun request ->
                expectConfigurationFailure (fun () ->
                    resolve
                        (canonical [| request |])
                        [| identity "package" "1.0.1" |]
                )
            )
        }
    ]
