#!/bin/bash
echo "FileAccess Accept tests are starting .... "
echo "Wait for 10 seconds"
sleep 10s

echo "Integration tests starting...."
dotnet vstest IntegrationTests/IntegrationTests.dll --logger:trx
cp TestResults/*.trx testresults/IntegrationTests.trx
rm TestResults/*.trx
echo "Integration tests finished"
echo " "
echo " "
echo " All acceptance tests completed"
echo " "

