#!/bin/bash
echo "Filter AcceptanceTests are starting .... "
rm -rf testresults
mkdir testresults
echo "List out files in current directory"
ls

echo "Wait for 20 seconds"
sleep 20s

echo "Run the AcceptanceTests solution"
cd AcceptanceTests
dotnet test VSS.Visionlink.Filter.AcceptanceTests.sln --logger \"nunit;LogFileName=acceptancetestresults.xml\"
echo " "
echo " All acceptance tests completed"
echo " "

