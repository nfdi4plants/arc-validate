#!/usr/bin/env python

import anybadge

# Define thresholds: <2=red, <4=orange <8=yellow <10=green
thresholds = {2: 'red',
              4: 'orange',
              6: 'yellow',
              10: 'green'}

badge = anybadge.Badge(
    label='arc quality',
    value=9, 
    value_suffix="/10",
    thresholds=thresholds
)

badge.write_badge('arc-quality.svg')