#!/bin/bash

rm -rf artifacts

dotnet publish ./src/MasterDataConsumer -o artifacts/MasterDataConsumer -f netcoreapp1.0 -c Docker
dotnet publish ./src/ProjectWebApi -o artifacts/ProjectWebApi -f netcoreapp1.0 -c Docker

cp src/MasterDataConsumer/appsettings.json artifacts/MasterDataConsumer/
cp src/ProjectWebApi/appsettings.json artifacts/ProjectWebApi/

mkdir artifacts/Logs