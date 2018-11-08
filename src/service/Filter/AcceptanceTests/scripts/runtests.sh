#!/bin/bash
echo "Accept tests are starting .... "
echo "Wait for 30 seconds"
sleep 30s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh db:3306 -t 0
/bin/bash wait-for-it.sh kafka:9092 -t 0
echo "Wait for 30 seconds"
sleep 30s

echo "WebApiTests starting...."
dotnet vstest WebApiTests/WebApiTests.dll --logger:trx
cp TestResults/*.trx testresults/WebApiTests.trx
rm TestResults/*.trx
echo "WebApiTests finished"

echo "Run Executor tests starting...."
dotnet vstest ExecutorTests/ExecutorTests.dll --logger:trx
cp TestResults/*.trx testresults/ExecutorTests.trx
rm TestResults/*.trx
echo "Executor tests finished"

echo "Run Repository tests starting...."
dotnet vstest RepositoryTests/RepositoryTests.dll --logger:trx
cp TestResults/*.trx testresults/RepositoryTests.trx
rm TestResults/*.trx
echo "Repository tests finished"

echo " "
echo " "
echo " All acceptance tests completed"
echo " "
