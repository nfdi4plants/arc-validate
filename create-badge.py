#!/usr/bin/env python
import anybadge
from junitparser import TestCase, Failure, Error, JUnitXml

xml = JUnitXml.fromfile('./arc-validate-results.xml')

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

badge.write_badge('arc-quality.svg')