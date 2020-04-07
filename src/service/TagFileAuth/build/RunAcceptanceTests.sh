#!/bin/bash
echo "TagFileAuth AcceptanceTests are starting .... "
rm -rf testresults
mkdir testresults
echo "List out files in current directory"
ls

echo "Wait for 20 seconds"
sleep 20s

echo "Run the AcceptanceTests solution"
cd AcceptanceTests
#dotnet publish --force VSS.TagFileAuth.Service.AcceptanceTests.sln
dotnet test VSS.TagFileAuth.Service.AcceptanceTests.sln --logger \"nunit;LogFileName=acceptancetestresults.xml\"  --diag testlog.log

echo " "
echo " All acceptance tests completed"
echo " "
