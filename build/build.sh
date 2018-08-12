#!/bin/bash

rm -rf artifacts

#dotnet publish ../VSS.Productivity3D.Filter.sln -o ../../artifacts -f netcoreapp2.0 --runtime linux-x64

dotnet publish ../src/VSS.Productivity3D.Filter.Cleanup/VSS.Productivity3D.Filter.Cleanup.csproj -o ../../artifacts -f netcoreapp2.0
dotnet publish ../src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o ../../artifacts -f netcoreapp2.0

#cp ../src/VSS.Productivity3D.Filter.Cleanup/appsettings.json artifacts/VSS.Productivity3D.Filter.Cleanup/
cp ../src/VSS.Productivity3D.Filter.WebApi/appsettings.json artifacts/VSS.Productivity3D.Filter.WebApi/

mkdir artifacts/logs