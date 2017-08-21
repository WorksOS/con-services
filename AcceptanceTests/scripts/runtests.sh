#!/bin/bash
echo "Accept tests are starting .... "
echo "Wait for 50 seconds"
sleep 50s
echo "Check the database to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh db:3306 -t 0
#/bin/bash wait-for-it.sh kafka:9092 -t 0
echo "Wait for 30 seconds"
sleep 30s
# Run the component tests
echo "Run the component tests"

echo "Run the component/acceptance tests"
echo "ExecutorTests starting...."
dotnet TestRun/TestRun.dll results=/testresults/ExecutorTestsResults project=ExecutorTests messsages=false
echo "ExecutorTests finished"

echo "RepositoryTests starting...."
dotnet TestRun/TestRun.dll results=/testresults/RepositoryTestsResults project=RepositoryTests messsages=false
echo "RepositoryTests finished"

echo "WebApiTests starting...."
dotnet TestRun/TestRun.dll results=/testresults/WebApiTestsResults project=WebApiTests messsages=false
echo "WebApiTests finished"

echo "EventTests event tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/EventTestsResults project=EventTests messsages=false
echo "EventTests event tests finished"

echo "Run Integration tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/IntegrationTestResults project=IntegrationTests messsages=false
echo "Integration tests finished"

echo " "
echo " "
echo " All acceptance tests completed"
echo " "

