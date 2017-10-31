#!/bin/bash
echo "Accept tests are starting .... "
echo "Wait for 90 seconds"
sleep 90s
echo "Check the database and kafka to see if port is available"
# Polling the database and kafka status before test
/bin/bash wait-for-it.sh db:3306 -t 0
#/bin/bash wait-for-it.sh kafka:9092 -t 0
echo "Wait for 30 seconds"
sleep 30s
# Run the component tests
echo "Run the component tests"

echo "Run Scheduler tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/SchedulerTestResults project=SchedulerTests messages=false
echo "Scheduler tests finished"

echo "Run Repository tests starting...."
dotnet TestRun/TestRun.dll results=/testresults/RepositoryTestResults project=RepositoryTests messages=false
echo "Repository tests finished"

echo " "
echo " "
echo " All acceptance tests completed"
echo " "

