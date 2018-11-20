#!/bin/bash
echo "Scheduler AcceptanceTests are starting .... "
rm -rf testresults
mkdir testresults

echo "Wait for 210 seconds"
sleep 210s
#echo "Check the database to see if port is available"
# Polling the database and kafka status before test
#/bin/bash wait-for-it.sh localhost:3306 -t 0
#/bin/bash wait-for-it.sh localhost:9092 -t 0

cd AcceptanceTests
echo "List out files in AcceptanceTests directory"
ls
echo "Run the AcceptanceTests solution"
dotnet test VSS.Productivity3D.Scheduler.AcceptanceTests.sln --logger \"xunit;LogFileName=acceptancetestresults.xml\"
echo " "
echo " All acceptance tests completed"
echo " "

