#!/bin/bash
echo "Accept tests are starting .... "
echo "Wait for 50 seconds"
sleep 50s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh db:3306 -t 0
/bin/bash wait-for-it.sh kafka:9092 -t 0
echo "Wait for 10 seconds"
sleep 10s
# Run the component tests
echo "Run the component tests"
echo "KafkaTests starting...."
dotnet TestRun/TestRun.dll results=/testresults/KafkaTestsResults project=KafkaTests
echo "KafkaTests finished"

echo "RepositoryTests starting...."
dotnet TestRun/TestRun.dll results=/testresults/RepositoryTestsResults project=RepositoryTests
echo "RepositoryTests finished"

echo "Run the component/acceptance tests"
echo "EventTests event tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/EventTestsResults project=EventTests
echo "EventTests event tests finished"

echo "WebApiTests starting...."
dotnet TestRun/TestRun.dll results=/testresults/WebApiTestsResults project=WebApiTests
echo "WebApiTests finished"

echo "Run Integration tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/IntegrationTestResults project=IntegrationTests
echo "Integration tests finished"
echo " "
echo " "
echo " All acceptance tests completed"
echo " "

