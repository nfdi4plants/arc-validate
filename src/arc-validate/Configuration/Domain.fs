namespace ARCValidate.Configuration

open ValidationPackage.Model

/// Identifies a validation-configuration failure that should be reported as exit code 4.
exception ConfigurationException of message: string

/// Describes the version policy recorded in an executable validation plan.
[<RequireQualifiedAccess>]
type ExecutionPlanRollForward =
    | Disable
    | LatestPatch
    | LatestMinor
    | LegacyLatestStable

/// Describes one package selection after registry version resolution.
type ResolvedValidationPackage =
    {
        Name: string
        RequestedVersion: SemVer option
        RollForward: ExecutionPlanRollForward
        ResolvedVersion: SemVer
        Selection: ValidationPackageSelection option
    }

/// Holds a decoded configuration after every package version has been resolved.
type ResolvedValidationConfig =
    {
        ArcSpecification: SemVer option
        ValidationPackages: ResolvedValidationPackage array
        IsLegacy: bool
    }

/// Describes one package identity in the CLI-owned execution-plan contract.
type ExecutionPlanSelection =
    {
        Name: string
        RequestedVersion: string option
        RollForward: ExecutionPlanRollForward
        ResolvedVersion: string
    }

/// Describes the complete, preflighted validation plan emitted by the CLI.
type ExecutionPlan =
    {
        ConfigSha256: string
        ArcSpecification: string option
        ValidationPackages: ExecutionPlanSelection array
    }

/// Contains the exact decoded file and digest needed by the resolver.
type LoadedValidationConfig =
    {
        Path: string
        Sha256: string
        Decoded: ValidationPackage.Codecs.DecodedValidationPackagesConfig
    }
