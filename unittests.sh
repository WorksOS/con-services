#!/bin/bash
(cd ./test/UnitTests/MasterDataProjectTests && dotnet test VSS.Project.WebApi.Tests.csproj -f netcoreapp2.0 )
if [ $? -ne 0 ]; then exit 1
fi

