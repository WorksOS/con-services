#!/bin/bash

rm -rf artifacts
rm -rf ./backup*

dotnet publish ./src/WebApi -o ../../artifacts/WebApi -f netcoreapp1.1 -c Docker

cp src/WebApi/appsettings.json artifacts/WebApi/
cp src/WebApi/bin/Docker/netcoreapp1.1/VSS.Productivity3D.TagFileAuth.WebAPI.xml artifacts/WebApi/

mkdir artifacts/logs