#!/bin/bash
echo "Accept tests are starting .... "
echo "Wait for 10 seconds"
sleep 10s

cd /app
echo "Run Integration tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/IntegrationTestResults project=IntegrationTests
echo "Integration tests finished"

echo " "
echo " "
echo " All acceptance tests completed"
echo " "

