#!/bin/bash

rm -rf artifacts

dotnet publish ../src/VSS.Productivity3D.Scheduler.WebApi/VSS.Productivity3D.Scheduler.WebApi.csproj -o ../../artifacts -f netcoreapp2.0

cp ../src/VSS.Productivity3D.Scheduler.WebApi/appsettings.json artifacts/VSS.Productivity3D.Scheduler.WebApi/

mkdir artifacts/logs