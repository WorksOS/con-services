#!/bin/bash

rm -rf artifacts

dotnet publish ./src/MasterDataConsumer.csproj -o ../../artifacts/MasterDataConsumer -f netcoreapp1.1 -c Docker

cp src/MasterDataConsumer/appsettings.json artifacts/MasterDataConsumer/

mkdir artifacts/logs