#!/bin/bash

rm -rf artifacts

dotnet publish ./src/MasterDataConsumer/VSS.Productivity3D.MasterDataConsumer.csproj -o ../../artifacts/MasterDataConsumer -f netcoreapp3.1 -c Docker

cp src/MasterDataConsumer/appsettings.json artifacts/MasterDataConsumer/

mkdir artifacts/logs