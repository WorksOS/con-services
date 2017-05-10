#!/bin/bash
echo "Accept tests are starting .... "
echo "Wait for 60 seconds"
sleep 60s
# Run the component tests
echo "Run the component tests"
echo "Run Integration tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/IntegrationTestResults project=IntegrationTests
echo "Integration tests finished"
echo " "
echo " "
echo " All acceptance tests completed"
echo " "

