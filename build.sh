#!/bin/bash

rm -rf artifacts

dotnet publish ./src/WebApi -o ../../artifacts/WebApi -f netcoreapp1.1 -c Docker

cp src/WebApi/appsettings.json artifacts/WebApi/
cp src/WebApi/bin/Docker/netcoreapp1.1/WebApi.xml artifacts/WebApi/

mkdir artifacts/logs