#!/bin/bash
echo "Project AcceptanceTests are starting .... "
rm -rf testresults
mkdir testresults
echo "List out files in current directory"
ls

echo "Wait 40 seconds for MySQL"
sleep 40


echo "$(date) Run the AcceptanceTests solution"
cd AcceptanceTests
dotnet test VSS.Visionlink.Project.AcceptanceTests.sln --logger \"nunit;LogFileName=acceptancetestresults.xml\"

echo " "
echo " All acceptance tests completed"
echo " "
