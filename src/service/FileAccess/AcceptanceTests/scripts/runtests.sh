#!/bin/bash
echo "FileAccess Accept tests are starting .... "
echo "Wait for 50 seconds"
sleep 50s

cd /app
echo "Integration tests starting...."
dotnet vstest IntegrationTests/IntegrationTests.dll --logger:trx
cp TestResults/*.trx testresults/IntegrationTests.trx
rm TestResults/*.trx
echo "Integration tests finished"
echo " "
echo " "
echo " All acceptance tests completed"
echo " "

