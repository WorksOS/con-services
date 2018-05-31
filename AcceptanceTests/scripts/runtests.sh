#!/bin/bash
echo "Accept tests are starting .... "
echo "Wait for 60 seconds"
sleep 60s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh localhost:3306 -t 0
/bin/bash wait-for-it.sh localhost:9092 -t 0
echo "Wait for 120 seconds"
sleep 120s
# Run the component tests
echo "Run the component tests"

echo "WebApiTests starting...."
dotnet TestRun/TestRun.dll results=/testresults/WebApiTestsResults project=WebApiTests
echo "WebApiTests finished"

echo "Run Integration tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/IntegrationTestResults project=IntegrationTests
echo "Integration tests finished"

echo "Run Executor tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/ExecutorTestResults project=ExecutorTests
echo "Executor tests finished"
echo " "
echo " "
echo " All acceptance tests completed"
echo " "

