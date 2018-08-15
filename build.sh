#!/bin/bash

rm -rf artifacts

dotnet publish ./src/VSS.Productivity3D.Scheduler.WebApi/VSS.Productivity3D.Scheduler.WebApi.csproj -o ../../artifacts/VSS.Productivity3D.Scheduler.WebApi -f netcoreapp2.0

cp src/VSS.Productivity3D.Scheduler.WebApi/appsettings.json artifacts/VSS.Productivity3D.Scheduler.WebApi/
cp src/VSS.Productivity3D.Scheduler.WebApi/Dockerfile artifacts/VSS.Productivity3D.Scheduler.WebApi/Dockerfile
cp src/VSS.Productivity3D.Scheduler.WebApi/bin/Docker/netcoreapp2.0/VSS.Productivity3D.Scheduler.WebAPI.xml artifacts/VSS.Productivity3D.Scheduler.WebApi/

mkdir artifacts/logs