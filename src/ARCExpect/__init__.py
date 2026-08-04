"""Portable ARC validation contracts, Pyxpecto cases, and execution."""

from fable_library.async_ import await_task, delay, start_as_task
from fable_library.list import of_array

from .arcvalidation_package import ARCValidationPackage
from .badge import Writer as _BadgeWriter
from .execution import Execute as _PortableExecute
from .fable_modules.fable_pyxpecto.pyxpecto import (
    Test_ftestCase,
    Test_ftestCaseAsync,
    Test_ftestList,
    Test_ptestCase,
    Test_ptestCaseAsync,
    Test_ptestList,
    Test_testCase,
    Test_testCaseAsync,
    Test_testList,
)
from .junit import Writer as _JUnitWriter
from .package_arguments import PackageArguments
from .results import CaseOutcome, CaseResult, RunSummary, ValidationResult
from .setup import (
    FrontmatterLanguage_FSharpFrontmatter,
    FrontmatterLanguage_PythonFrontmatter,
    Setup,
)
from .validation_summary import ValidationPackageSummary, ValidationSummary


class Badge:
    Writer = _BadgeWriter


class JUnit:
    Writer = _JUnitWriter


class FrontmatterLanguage:
    FSharpFrontmatter = FrontmatterLanguage_FSharpFrontmatter.singleton
    PythonFrontmatter = FrontmatterLanguage_PythonFrontmatter.singleton


class Execute:
    @staticmethod
    async def validation(
        validation_package,
        payload=None,
        source_branch=None,
        source_commit_hash=None,
        arguments=None,
    ):
        return await start_as_task(
            _PortableExecute.Validation(
                validation_package,
                payload,
                source_branch,
                source_commit_hash,
                arguments,
            )
        )

    Validation = validation


def test_case(name, body):
    return Test_testCase(name, body)


def ptest_case(name, body):
    return Test_ptestCase(name, body)


def ftest_case(name, body):
    return Test_ftestCase(name, body)


def test_case_async(name, body):
    return Test_testCaseAsync(name, delay(lambda: await_task(body())))


def ptest_case_async(name, body):
    return Test_ptestCaseAsync(name, delay(lambda: await_task(body())))


def ftest_case_async(name, body):
    return Test_ftestCaseAsync(name, delay(lambda: await_task(body())))


def test_list(name, tests):
    return Test_testList(name, of_array(tests))


def ptest_list(name, tests):
    return Test_ptestList(name, of_array(tests))


def ftest_list(name, tests):
    return Test_ftestList(name, of_array(tests))


__all__ = [
    "ARCValidationPackage",
    "Badge",
    "CaseOutcome",
    "CaseResult",
    "Execute",
    "FrontmatterLanguage",
    "JUnit",
    "PackageArguments",
    "RunSummary",
    "Setup",
    "ValidationPackageSummary",
    "ValidationResult",
    "ValidationSummary",
    "ftest_case",
    "ftest_case_async",
    "ftest_list",
    "ptest_case",
    "ptest_case_async",
    "ptest_list",
    "test_case",
    "test_case_async",
    "test_list",
]
