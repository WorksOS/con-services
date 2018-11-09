#!/bin/bash

rm -rf artifacts

dotnet publish ../src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o ../../artifacts -f netcoreapp2.0
dotnet publish ../src/VSS.Productivity3D.Filter.Cleanup/VSS.Productivity3D.Filter.Cleanup.csproj -o ../../artifacts -f netcoreapp2.0

mkdir artifacts/logs