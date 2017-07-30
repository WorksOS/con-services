#!/bin/bash

rm -rf artifacts

dotnet publish ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o ../../artifacts/FilterWebApi -f netcoreapp1.1

cp src/VSS.Productivity3D.Filter.WebApi/appsettings.json artifacts/FilterWebApi/
cp src/VSS.Productivity3D.Filter.WebApi/Dockerfile artifacts/FilterWebApi/Dockerfile

mkdir artifacts/logs