#!/bin/bash
(cd ./test/UnitTests/MasterDataConsumerTests && dotnet test -f netcoreapp1.0 )
if [ $? -ne 0 ]; then exit 1
fi

