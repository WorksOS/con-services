#!/bin/bash

(cd ./test/UnitTests/MasterDataProjectTests && dotnet test VSS.Project.WebApi.Tests.csproj --logger:\"xunit;LogFilePath=/TestResults/TestResults.xml\"  )

if [ $? -ne 0 ]; then exit 1
fi

