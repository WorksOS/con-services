#!/bin/bash
(cd ./test/UnitTests/MasterDataConsumerTests && dotnet test MasterDataConsumerTests.csproj -f netcoreapp1.1 )
if [ $? -ne 0 ]; then exit 1
fi

