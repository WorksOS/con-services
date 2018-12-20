#!/bin/bash
echo "Filter AcceptanceTests are starting .... "
rm -rf testresults
mkdir testresults
echo "List out files in current directory"
ls

echo "Wait for 20 seconds"
sleep 20s

echo "Check the database and kafka to see if port is available"

echo "Run the AcceptanceTests solution"
cd AcceptanceTests
dotnet test VSS.Visionlink.Filter.AcceptanceTests.sln --logger \"xunit;LogFileName=acceptancetestresults.xml\"
echo " "
echo " All acceptance tests completed"
echo " "
