#!/bin/bash
echo "Scheduler Accept tests are starting .... "
echo "Wait for 120 seconds"
sleep 120s
echo "Check the database ports are available"
# Polling the database status before test
/bin/bash wait-for-it.sh db:3306 -t 0

echo "Run Repository tests starting...."
dotnet vstest RepositoryTests/RepositoryTests.dll --logger:trx
cp TestResults/*.trx testresults/RepositoryTests.trx
rm TestResults/*.trx
echo "Repository tests finished"

echo "Run WebApi tests starting...."
dotnet vstest WebApiTests/WebApiTests.dll --logger:trx
cp TestResults/*.trx testresults/WebApiTests.trx
rm TestResults/*.trx
echo "WebApi tests finished"

echo " "
echo " "
echo " All acceptance tests completed"
echo " "

