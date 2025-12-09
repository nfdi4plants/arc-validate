from dataclasses import dataclass

@dataclass
class SemVer:
    """Represents the semantic version of a validation package"""
    Major: int = -1
    Minor: int = -1
    Patch: int = -1
    PreRelease: str = ""
    BuildMetadata: str = ""

    @staticmethod
    def try_parse(version: str):
        match = globals.SEMVER_REGEX.match(version)
        if not match:
            raise ValueError(f"Invalid semantic version: {version}")

        return SemVer(
            Major=int(match.group("major")) or -1,
            Minor=int(match.group("minor")) or -1,
            Patch=int(match.group("patch")) or -1,
            PreRelease=match.group("prerelease") or "",
            BuildMetadata=match.group("buildmetadata") or "",
        )

    @staticmethod
    def toString(semver:SemVer) -> str:
        if semver.BuildMetadata is not '':
            if semver.PreRelease is not '':
                return f'{semver.Major}.{semver.Minor}.{semver.Patch}-{semver.PreRelease}+{semver.BuildMetadata}'
            else:
                return f'{semver.Major}.{semver.Minor}.{semver.Patch}+{semver.BuildMetadata}'
        else:
            if semver.PreRelease is not '':
                return f'{semver.Major}.{semver.Minor}.{semver.Patch}-{semver.PreRelease}'
            else:
                return f'{semver.Major}.{semver.Minor}.{semver.Patch}'

@dataclass 
class Author:
    """"""
    FullName = "" 
    Email = ""
    Affiliation = ""
    AffiliationLink = ""

@dataclass 
class OntologyAnnotation:
    """"""
    Name: str = "" 
    TermSourceREF: str = ""
    TermAccessionNumber: str = ""

@dataclass
class ValidationPackageMetadata:
    """"""
    Name: str = "" 
    Summary: str = ""
    Description: str = ""
    MajorVersion: int = -1
    MinorVersion: int = -1
    PatchVersion: int = -1
    PreReleaseVersionSuffix: str = ""
    BuildMetadataVersionSuffix: str = ""
    ProgrammingLanguage: str = ""

    # optional fields
    Publish: bool = False
    Authors: list [Author] = []
    Tags: list [OntologyAnnotation] = []
    ReleaseNotes: str = ""
    CQCHookEndpoint: str = ""

    @staticmethod
    def tryGetSemanticVersion(m: ValidationPackageMetadata):
        SemVer.try_parse(
            SemVer.toString(
                SemVer(
                    m.MajorVersion,
                    m.MinorVersion,
                    m.PatchVersion,
                    m.PreReleaseVersionSuffix,
                    m.BuildMetadataVersionSuffix
                )
            ) # there is no built-in validation on the constructor/create function, so we'll take a detour via parsing roundtrip using the regex
        )
        
    @staticmethod
    def tryGetSemanticVersionString(m: ValidationPackageMetadata):
        SemVer.toString(ValidationPackageMetadata.tryGetSemanticVersion(m))
        