#!/bin/bash
echo "Project AcceptanceTests are starting .... "
echo "Wait for 55 seconds"
sleep 55s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh db:3306 -t 0
/bin/bash wait-for-it.sh kafka:9092 -t 0
sleep 55s



dotnet vstest ExecutorTests/ExecutorTests.dll --logger:trx
cp TestResults/*.trx testresults/ExecutorTests.trx
rm TestResults/*.trx

dotnet vstest IntegrationTests/IntegrationTests.dll --logger:trx
cp TestResults/*.trx testresults/IntegrationTests.trx
rm TestResults/*.trx

dotnet vstest WebApiTests/WebApiTests.dll --logger:trx
cp TestResults/*.trx testresults/WebApiTests.trx
rm TestResults/*.trx
echo " "
echo " All acceptance tests completed"
echo " "
sleep 500s

