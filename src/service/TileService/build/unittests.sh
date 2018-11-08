#!/bin/bash

(cd ./test/UnitTests/VSS.Tile.Service.Tests && dotnet test VSS.Tile.Service.WebApi.Tests.csproj \
  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutputDirectory=/TestResults/TestCoverage --logger:\"xunit;LogFilePath=/TestResults/TestResults.xml\")

if [ $? -ne 0 ]; then exit 1
fi

