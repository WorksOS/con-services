#!/bin/bash

rm -rf artifacts

dotnet publish ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o ../../artifacts/VSS.Productivity3D.Filter.WebApi -f netcoreapp2.0

cp src/VSS.Productivity3D.Filter.WebApi/appsettings.json artifacts/VSS.Productivity3D.Filter.WebApi/
cp src/VSS.Productivity3D.Filter.WebApi/Dockerfile artifacts/VSS.Productivity3D.Filter.WebApi/Dockerfile
cp src/VSS.Productivity3D.Filter.WebApi/bin/Docker/netcoreapp1.1/VSS.Productivity3D.Filter.WebAPI.xml artifacts/VSS.Productivity3D.Filter.WebApi/
mkdir artifacts/logs