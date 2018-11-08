#!/bin/bash
echo "Tile Service AcceptanceTests are starting .... "
rm -rf testresults
mkdir testresults
echo "List out files in current directory"
ls

echo "Wait for 20 seconds"
sleep 20s

echo "Run the AcceptanceTests solution"
cd AcceptanceTests
#dotnet publish --force VSS.Tile.Service.AcceptanceTests.sln
dotnet test VSS.Tile.Service.AcceptanceTests.sln --logger \"xunit;LogFileName=acceptancetestresults.xml\"

echo " "
echo " All acceptance tests completed"
echo " "
#echo " Wait for 300"
#sleep 300s
