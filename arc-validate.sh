#!/bin/bash
set -euxo pipefail
dotnet run --project /opt/arc-validate/src/arc-validate.fsproj --nunit-summary arc-validate-results.xml