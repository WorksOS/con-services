#!/bin/bash
echo "Project AcceptanceTests are starting .... "
echo "Wait for 20 seconds"
sleep 20s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
/bin/sh wait-for-it.sh localhost:3306 -t 0
/bin/sh wait-for-it.sh localhost:9092 -t 0
echo "Wait for 20 seconds"
sleep 20s

echo "Run the AcceptanceTests solution"
(cd ./AcceptanceTests && dotnet test VSS.Visionlink.Project.AcceptanceTests.sln --logger:\"xunit;LogFilePath=/TestResults/TestResults.xml\")
echo " "
echo " All acceptance tests completed"
echo " "

