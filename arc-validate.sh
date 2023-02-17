#!/bin/bash
set -euxo pipefail
dotnet run --project /opt/arc-validate/src/arc-validate.fsproj --junit-summary arc-validate-results.xml