#!/bin/bash

rm -rf artifacts

dotnet publish ./src/WebApi -o artifacts/WebApi -f netcoreapp1.1 -c Docker

cp src/WebApi/appsettings.json artifacts/WebApi/

mkdir artifacts/logs