#!/bin/bash
echo "AcceptanceTests are starting, wait 30 seconds"
sleep 30s

echo "Checking database availability..."
/bin/bash wait-for-it.sh db:3306 -t 55
echo "Checking Kafak availability..."
/bin/bash wait-for-it.sh kafka:9092 -t 55

echo "IntegrationTests starting"
dotnet vstest IntegrationTests/IntegrationTests.dll --logger:xunit
cp TestResults/*.trx testresults/IntegrationTests.trx
rm TestResults/*.trx

echo " "
echo " All acceptance tests completed"
echo " "
