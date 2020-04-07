#!/bin/bash
echo "FileAccess AcceptanceTests are starting .... "
rm -rf testresults
mkdir testresults
echo "List out files in current directory"
ls

echo "Wait for 20 seconds"
sleep 20s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
#/bin/bash wait-for-it.sh localhost:3306 -t 0
#/bin/bash wait-for-it.sh localhost:9092 -t 0
#echo "Wait for 20 seconds"
#sleep 20s

echo "Run the AcceptanceTests solution"
cd AcceptanceTests
dotnet test FileAccessAcceptanceTests.sln --logger \"nunit;LogFileName=acceptancetestresults.xml\"

echo " "
echo " All acceptance tests completed"
echo " "
#echo " Wait for 300"
#sleep 300s