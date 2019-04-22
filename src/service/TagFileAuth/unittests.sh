#!/bin/bash
(cd ./test/UnitTests/WebApiTests && dotnet test WebApiTests.csproj -f netcoreapp2.1 )
if [ $? -ne 0 ]; then exit 1
fi

