#!/bin/bash
echo "Accept tests are starting .... "
echo "Wait for 120 seconds"
sleep 120s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh db:3306 -t 0
/bin/bash wait-for-it.sh dbmssql:1433 -t 0
#/bin/bash wait-for-it.sh kafka:9092 -t 0
echo "Wait for 120 seconds"
sleep 120s
# Run the component tests
echo "Run the component tests"

echo "Run SchedulerTestsFilterCleanup tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/SchedulerTestsFilterCleanupTestResults project=SchedulerTestsFilterCleanup messages=false
echo "SchedulerTestsFilterCleanup tests finished"

echo "Run SchedulerTestsImportedFileSync tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/SchedulerTestsImportedFileSyncResults project=SchedulerTestsImportedFileSync messages=false
echo "SchedulerTestsImportedFileSync tests finished"

echo "Run Repository tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/RepositoryTestResults project=RepositoryTests messages=false
echo "Repository tests finished"

echo "Run WebApi tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/WebApiTestResults project=WebApiTests messages=false
echo "WebApi tests finished"

echo " "
echo " "
echo " All acceptance tests completed"
echo " "

