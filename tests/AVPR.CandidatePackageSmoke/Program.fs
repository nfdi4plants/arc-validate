module AVPR.CandidatePackageSmoke.Program

open System
open AVPRClient.Interop
open ValidationPackage.Codecs
open ValidationPackage.Model

let private require condition message =
    if not condition then
        failwith message

[<EntryPoint>]
let main _ =
    let generated =
        AVPRClient.ValidationPackage(
            Name = "candidate-contract",
            Summary = "Candidate contract",
            Description = "Exercises candidate AVPR packages.",
            MajorVersion = 1,
            MinorVersion = 2,
            PatchVersion = 3,
            PreReleaseVersionSuffix = "rc.1",
            BuildMetadataVersionSuffix = "build.7",
            ProgrammingLanguage = "FSharp",
            Inputs = ResizeArray [
                AVPRClient.CommandInputParameter(
                    Id = "arc-directory",
                    Type = AVPRClient.CommandInputType.String_,
                    Label = "ARC directory",
                    Doc = "Path to the ARC.",
                    InputBinding =
                        AVPRClient.CommandInputBinding(
                            Position = 1,
                            Prefix = "--arc-directory",
                            Separate = true
                        )
                )
            ]
        )

    let model = generated.ToModel()
    let input = model.Inputs[0]

    require (input.Id = "arc-directory") "Generated-client input id drifted."
    require (input.Type.PrimitiveType = CwlPrimitive.String) "CWL primitive drifted."
    require input.Type.IsNullable "CWL nullability drifted."
    require (input.InputBinding.Prefix = "--arc-directory") "CWL prefix drifted."

    let frontmatter =
        """(*
---
Name: candidate-contract
Summary: Candidate contract
Description: Exercises candidate AVPR packages.
MajorVersion: 1
MinorVersion: 2
PatchVersion: 3
PreReleaseVersionSuffix: rc.1
BuildMetadataVersionSuffix: build.7
ProgrammingLanguage: FSharp
Inputs:
  - id: arc-directory
    type: string?
    label: ARC directory
    doc: Path to the ARC.
    inputBinding:
      position: 1
      prefix: --arc-directory
      separate: true
---
*)"""

    let decoded =
        ValidationPackageYaml.extractOrFail
            FrontmatterLanguage.FSharp
            frontmatter

    require (decoded = model) "Frontmatter/model candidate contract drifted."

    let jsonRoundTrip =
        model
        |> ValidationPackageJson.encode
        |> ValidationPackageJson.decodeOrFail

    require (jsonRoundTrip = model) "JSON/model candidate contract drifted."

    let generatedRoundTrip =
        model.ToClient(Array.empty, DateTimeOffset.UnixEpoch)

    require
        (generatedRoundTrip.Inputs |> Seq.head).InputBinding.Separate
        "Model/client candidate contract drifted."

    0