#!/bin/bash
echo "Project AcceptanceTests are starting .... "
rm -rf testresults
mkdir testresults
echo "List out files in current directory"
ls

echo "Run the AcceptanceTests solution"
cd AcceptanceTests
echo "No Acceptance tests"
#dotnet publish --force VSS.Visionlink.Project.AcceptanceTests.sln
#dotnet test VSS.Visionlink.Project.AcceptanceTests.sln --logger \"xunit;LogFileName=acceptancetestresults.xml\"

echo " "
echo " All acceptance tests completed"
echo " "
#echo " Wait for 300"
#sleep 300s
