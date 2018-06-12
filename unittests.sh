#!/bin/bash

(cd ./test/UnitTests/MasterDataProjectTests && dotnet test VSS.Project.WebApi.Tests.csproj  --logger:\"xunit;LogFilePath=/TestResults/TestResults.xml\" \
 /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutputDirectory=/TestCoverage/
 && cp -R TestResults /build  )

if [ $? -ne 0 ]; then exit 1
fi

