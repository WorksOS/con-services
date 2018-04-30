#!/bin/bash
(cd ./test/UnitTests/MasterDataConsumerTests && dotnet test VSS.Productivity3D.MasterDataConsumer.Tests.csproj -f netcoreapp2.0 )
if [ $? -ne 0 ]; then exit 1
fi

