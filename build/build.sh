#!/bin/bash

rm -rf artifacts
rm -rf ./backup*

dotnet publish ./src/WebApi/VSS.Productivity3D.TagFileAuth.WebAPI.csproj -o ../../artifacts/WebApi -f netcoreapp2.0 -c Docker

cp src/WebApi/appsettings.json artifacts/WebApi/
cp src/WebApi/bin/Docker/netcoreapp2.0/VSS.Productivity3D.TagFileAuth.WebAPI.xml artifacts/WebApi/

mkdir artifacts/logs