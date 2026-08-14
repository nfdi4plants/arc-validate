"""Portable ARC validation contracts, Pyxpecto cases, and execution."""

import math
import operator
from pathlib import Path

from fable_library.async_ import await_task, delay, start_as_task
from fable_library.list import of_array

from .Common.Package.arcvalidation_package import ARCValidationPackage
from .Common.Outputs.badge import Color as _BadgeColor
from .Common.Outputs.badge import Threshold as _BadgeThreshold
from .Common.Outputs.badge import Writer as _BadgeWriter
from .Common.Execution.execution import Execute as _PortableExecute
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
from .fable_modules.thoth_json_core.types import (
    _Json,
    Json_Array,
    Json_Boolean,
    Json_Null,
    Json_Number,
    Json_Object,
    Json_String,
)
from .Common.Outputs.junit import Writer as _JUnitWriter
from .Common.Arguments.package_arguments import PackageArguments
from .Common.Results.results import CaseOutcome, CaseResult, RunSummary, ValidationResult
from .Common.Package.setup import Setup
from .Common.Results.validation_summary import ValidationPackageSummary, ValidationSummary
from .Common.Outputs.output_pipeline import PipelineOutputBundle as _PipelineOutputBundle


def _to_json(value):
    if isinstance(value, _Json):
        return value
    if value is None:
        return Json_Null.singleton
    if isinstance(value, bool):
        return Json_Boolean(value)
    if isinstance(value, (int, float)):
        number = float(value)
        if not math.isfinite(number):
            raise ValueError("ARCExpect payload numbers must be finite.")
        return Json_Number(number)
    try:
        integer = operator.index(value)
    except TypeError:
        pass
    else:
        number = float(integer)
        if not math.isfinite(number):
            raise ValueError("ARCExpect payload numbers must be finite.")
        return Json_Number(number)
    if isinstance(value, str):
        return Json_String(value)
    if isinstance(value, dict):
        fields = []
        for key, item in value.items():
            if not isinstance(key, str):
                raise TypeError("ARCExpect payload object keys must be strings.")
            fields.append((key, _to_json(item)))
        return Json_Object(of_array(fields))
    if isinstance(value, (list, tuple)):
        return Json_Array(of_array([_to_json(item) for item in value]))
    raise TypeError(
        "ARCExpect payload values must contain only dictionaries, lists, "
        "strings, numbers, booleans, or None."
    )


class Badge:
    Color = _BadgeColor
    Threshold = _BadgeThreshold
    Writer = _BadgeWriter


class JUnit:
    Writer = _JUnitWriter


class Execute:
    @staticmethod
    async def validation(
        validation_package,
        payload=None,
        source_branch=None,
        source_commit_hash=None,
        arguments=None,
    ):
        portable_payload = None if payload is None else _to_json(payload)
        return await start_as_task(
            _PortableExecute.Validation(
                validation_package,
                portable_payload,
                source_branch,
                source_commit_hash,
                arguments,
            )
        )

    Validation = validation

    @staticmethod
    def summary_creation(path, summary):
        Path(path).write_text(
            ValidationSummary.to_json(summary),
            encoding="utf-8",
            newline="",
        )

    SummaryCreation = summary_creation

    @staticmethod
    def junit_report_creation(path, summary, verbose=False):
        outputs = _PipelineOutputBundle.create(
            summary,
            VerboseJUnit=verbose,
        )
        Path(path).write_text(outputs.JUnitXml, encoding="utf-8", newline="")

    JUnitReportCreation = junit_report_creation

    @staticmethod
    def badge_creation(
        path,
        summary,
        label_text,
        value_suffix=None,
        thresholds=None,
        default_color=None,
    ):
        outputs = _PipelineOutputBundle.create(
            summary,
            BadgeLabelText=label_text,
            ValueSuffix=value_suffix,
            Thresholds=thresholds,
            DefaultColor=default_color,
        )
        Path(path).write_text(outputs.BadgeSvg, encoding="utf-8", newline="")

    BadgeCreation = badge_creation

    @staticmethod
    async def validation_pipeline(
        validation_package,
        arguments=None,
        badge_label_text=None,
        value_suffix=None,
        thresholds=None,
        default_color=None,
        verbose_junit=None,
        payload=None,
        source_branch=None,
        source_commit_hash=None,
    ):
        if arguments is None:
            arguments = PackageArguments.from_command_line(
                validation_package.Metadata
            )

        summary = await Execute.validation(
            validation_package,
            payload=payload,
            source_branch=source_branch,
            source_commit_hash=source_commit_hash,
            arguments=arguments,
        )
        outputs = _PipelineOutputBundle.create(
            summary,
            BadgeLabelText=badge_label_text,
            ValueSuffix=value_suffix,
            Thresholds=thresholds,
            DefaultColor=default_color,
            VerboseJUnit=verbose_junit,
        )
        result_folder = (
            Path(arguments.OutputDirectory)
            / outputs.ResultDirectoryName
            / outputs.FolderName
        )
        result_folder.mkdir(parents=True, exist_ok=True)
        (result_folder / outputs.SummaryFileName).write_text(
            outputs.SummaryJson,
            encoding="utf-8",
            newline="",
        )
        (result_folder / outputs.JUnitFileName).write_text(
            outputs.JUnitXml,
            encoding="utf-8",
            newline="",
        )
        (result_folder / outputs.BadgeFileName).write_text(
            outputs.BadgeSvg,
            encoding="utf-8",
            newline="",
        )
        return summary

    ValidationPipeline = validation_pipeline


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
