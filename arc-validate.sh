#!/bin/bash
set -euxo pipefail
# dotnet run --project /opt/arc-validate/src/arc-validate.fsproj --junit-summary arc-validate-results.xml
dotnet test /opt/arc-validate/src/arc-validate.fsproj --logger:"junit;LogFilePath=arc-validate-result.xml"
cp /opt/arc-validate/src/arc-validate-result.xml /arc/arc-validate-result.xml