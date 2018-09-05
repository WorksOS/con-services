#!/bin/bash

(cd ./test/UnitTests/VSS.Productivity3D.Scheduler.Tests && dotnet test VSS.Productivity3D.Scheduler.Tests.csproj \
  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutputDirectory=/TestResults/TestCoverage --logger:\"xunit;LogFilePath=/TestResults/TestResults.xml\")

if [ $? -ne 0 ]; then exit 1
fi