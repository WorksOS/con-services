#!/bin/bash
(cd ./test/UnitTests/WebApiTests && dotnet restore && dotnet test -f netcoreapp1.1 )
if [ $? -ne 0 ]; then exit 1
fi

