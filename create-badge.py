#!/usr/bin/python3
import anybadge
import os
from junitparser import TestCase, Failure, Error, JUnitXml

base_path = os.getenv('ARC_PATH')
xml_path = os.path.join(base_path,'arc-validate-results.xml')
output_path = os.path.join(base_path,'arc-quality.svg')
xml = JUnitXml.fromfile(xml_path)

passed = 0
failed = 0
errored = 0

for suites in xml:
    for case in suites:
        if case.is_passed: 
            passed += 1
        else:
            if isinstance(case.result[0], Error):
                errored += 1
            elif isinstance(case.result[0], Failure):
                failed += 1

# we need to discuss how to handle errored tests (e.g. tests that failed not because of the test case, but test execution)
total = passed + failed

# Define thresholds
thresholds = {1: 'darkgoldenrod',
            # come up with some logic for silver here
              total+1: 'gold'}

badge = anybadge.Badge(
    label='arc quality',
    value= passed, 
    value_suffix=f"/{total}",
    thresholds=thresholds
)

badge.write_badge(output_path)