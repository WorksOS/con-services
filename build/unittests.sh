#!/bin/bash
(cd ./test/UnitTests/WebApiTests && dotnet test WebApiTests.csproj \
  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=/TestResults/TestCoverage --logger:\"xunit;LogFilePath=/TestResults/TestResults.xml\")
if [ $? -ne 0 ]; then exit 1
fi

