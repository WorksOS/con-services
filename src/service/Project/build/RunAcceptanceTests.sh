#!/bin/bash
echo "Project AcceptanceTests are starting .... "
rm -rf testresults
mkdir testresults
echo "List out files in current directory"
ls

echo "Wait 40 seconds for MySQL and Kafka"
sleep 40
#echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
#/bin/bash wait-for-it.sh localhost:3306 -t 0
#/bin/bash wait-for-it.sh localhost:9092 -t 0
#echo "Wait for 20 seconds"
#sleep 20

echo "Run the AcceptanceTests solution"
cd AcceptanceTests
dotnet test VSS.Visionlink.Project.AcceptanceTests.sln --logger \"nunit;LogFileName=acceptancetestresults.xml\"

echo " "
echo " All acceptance tests completed"
echo " "
