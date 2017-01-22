#!/bin/bash

rm -rf Artifacts

dotnet publish ./src/MasterDataConsumer -o artifacts/MasterDataConsumer -f netcoreapp1.1 -c Docker
dotnet publish ./src/ProjectWebApi -o artifacts/ProjectWebApi -f netcoreapp1.1 -c Docker

cp src/MasterDataConsumer/appsettings.json Artifacts/MasterDataConsumer/
cp src/ProjectWebApi/appsettings.json Artifacts/ProjectWebApi/

mkdir Artifacts/Logs