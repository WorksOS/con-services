#!/bin/bash
echo "TFA Accept tests are starting .... "
echo "Wait for 20 seconds"
sleep 20s

cd /app
echo "WebApiTests starting...."
dotnet vstest WebApiTests/WebApiTests.dll --logger:trx
cp testresults/*.trx testresults/WebApiTests.trx
rm testresults/*.trx
echo "WebApiTests finished"

echo " "
echo " "
echo " All acceptance tests completed"
echo " "

