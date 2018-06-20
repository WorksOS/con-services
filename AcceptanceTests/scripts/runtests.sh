#!/bin/bash
echo "Project AcceptanceTests are starting .... "
echo "Wait for 55 seconds"
sleep 55s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh db:3306 -t 0
/bin/bash wait-for-it.sh kafka:9092 -t 0
sleep 55s



cd /app
dotnet vstest ExecutorTests/ExecutorTests.dll --logger:trx;LogFileName=ExecutorTestsTestResults --ResultsDirectory:/app/testresults
dotnet vstest IntegrationTests/IntegrationTests.dll --logger:trx;LogFileName=IntegrationTestsResults --ResultsDirectory:/app/testresults
dotnet vstest WebApiTests/WebApiTests.dll --logger:trx;LogFileName=WebApiTestsTestResults --ResultsDirectory:/app/testresults
echo " "
echo " All acceptance tests completed"
echo " "
sleep 500s

