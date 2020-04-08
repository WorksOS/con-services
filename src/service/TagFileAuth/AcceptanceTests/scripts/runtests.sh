#!/bin/bash
echo "TFA Accept tests are starting .... "
echo "Wait for 20 seconds"
sleep 20s
echo "Check the database to see if port is available"
# Polling the database status before test
/bin/bash wait-for-it.sh db:3306 -t 0
echo "Wait for 20 seconds"
sleep 20s

cd /app
echo "WebApiTests starting...."
dotnet vstest WebApiTests/WebApiTests.dll --logger:trx
cp TestResults/*.trx testresults/WebApiTests.trx
rm TestResults/*.trx
echo "WebApiTests finished"

echo " "
echo " "
echo " All acceptance tests completed"
echo " "
sleep 500s

