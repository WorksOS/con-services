#!/bin/bash
echo "AcceptanceTests are starting, wait 30 seconds"
sleep 30s

echo "Checking database availability..."
/bin/bash wait-for-it.sh db:3306 -t 55

echo "IntegrationTests starting"
dotnet vstest IntegrationTests/IntegrationTests.dll --logger:xunit
cp testresults/*.trx testresults/IntegrationTests.trx
rm testresults/*.trx

echo " "
echo " All acceptance tests completed"
echo " "
