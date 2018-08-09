#!/bin/bash

rm -rf artifacts

#dotnet publish ./src/VSS.Productivity3D.Filter.Cleanup/VSS.Productivity3D.Filter.Cleanup.csproj -o ../../artifacts/VSS.Productivity3D.Filter.Cleanup -f netcoreapp2.0
dotnet publish ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o ../../artifacts/VSS.Productivity3D.Filter.WebApi -f netcoreapp2.0

#cp src/VSS.Productivity3D.Filter.Cleanup/appsettings.json artifacts/VSS.Productivity3D.Filter.Cleanup/
cp src/VSS.Productivity3D.Filter.WebApi/appsettings.json artifacts/VSS.Productivity3D.Filter.WebApi/

mkdir artifacts/logs