#!/bin/bash
echo "AcceptanceTests are starting, wait 30 seconds"
sleep 30s

echo "Checking database availability..."
/bin/bash wait-for-it.sh db:3306 -t 55

echo "RepositoryTests starting"
dotnet vstest RepositoryTests/RepositoryTests.dll --logger:xunit
cp testresults/*.trx testresults/RepositoryTests.trx
rm testresults/*.trx

echo "WebApiTests starting"
dotnet vstest WebApiTests/WebApiTests.dll --logger:xunit
cp testresults/*.trx testresults/WebApiTests.trx
rm testresults/*.trx

echo " "
echo " All acceptance tests completed"
echo " "
