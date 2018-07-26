#!/bin/bash
echo "TFA Accept tests are starting .... "
echo "Wait for 50 seconds"
sleep 50s
echo "Check the database to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh db:3306 -t 0
/bin/bash wait-for-it.sh kafka:9092 -t 0
echo "Wait for 55 seconds"
sleep 55s

cd /app
echo "ExecutorTests starting...."
dotnet vstest ExecutorTests/ExecutorTests.dll --logger:trx
cp TestResults/*.trx testresults/ExecutorTests.trx
rm TestResults/*.trx
echo "ExecutorTests finished"

echo "RepositoryTests starting...."
dotnet vstest RepositoryTests/RepositoryTests.dll --logger:trx
cp TestResults/*.trx testresults/RepositoryTests.trx
rm TestResults/*.trx
echo "RepositoryTests finished"

echo "WebApiTests starting...."
dotnet vstest WebApiTests/WebApiTests.dll --logger:trx
cp TestResults/*.trx testresults/WebApiTests.trx
rm TestResults/*.trx
echo "WebApiTests finished"

echo "EventTests starting...."
dotnet vstest EventTests/EventTests.dll --logger:trx
cp TestResults/*.trx testresults/EventTests.trx
rm TestResults/*.trx
echo "EventTests finished"

echo "Integration tests starting...."
dotnet vstest IntegrationTests/IntegrationTests.dll --logger:trx
cp TestResults/*.trx testresults/IntegrationTests.trx
rm TestResults/*.trx
echo "Integration tests finished"

echo " "
echo " "
echo " All acceptance tests completed"
echo " "
sleep 500s

