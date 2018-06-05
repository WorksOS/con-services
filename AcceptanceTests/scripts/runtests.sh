#!/bin/bash
echo "Project AcceptanceTests are starting .... "
echo "Wait for 55 seconds"
sleep 55s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh db:3306 -t 0
/bin/bash wait-for-it.sh kafka:9092 -t 0
sleep 55s



cd /app
#dotnet test VSS.Visionlink.Project.AcceptanceTests.sln --logger:\"xunit;LogFilePath=/TestResults/ExecutorTestsTestResults.xml\"
dotnet vstest ExecutorTests/ExecutorTests.dll --logger:\"xunit;LogFilePath=/TestResults/ExecutorTestsTestResults.xml\"
cd ..
dotnet vstest IntegrationTests/IntegrationTests.dll --logger:\"xunit;LogFilePath=/TestResults/IntegrationTestsResults.xml\"

dotnet vstest WebApiTests/WebApiTests.dll --logger:\"xunit;LogFilePath=/TestResults/WebApiTestsTestResults.xml\"
echo " "
echo " All acceptance tests completed"
echo " "

