#!/bin/bash
(cd ./test/UnitTests/MasterDataConsumerTests && dotnet test VSS.Productivity3D.MasterDataConsumer.Tests.csproj \
  /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutputDirectory=/TestResults/TestCoverage --logger:\"xunit;LogFilePath=/TestResults/TestResults.xml\")
if [ $? -ne 0 ]; then exit 1
fi

