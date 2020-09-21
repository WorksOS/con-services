#!/bin/bash
echo "AcceptanceTests are starting, wait 10 seconds"
sleep 10s

echo "IntegrationTests starting"
dotnet test IntegrationTests/WebApiTests.dll --logger trx --results-directory AcceptanceTestResults

echo " "
echo " All acceptance tests completed"
echo " "
