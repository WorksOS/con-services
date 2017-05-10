#!/bin/bash

rm -rf artifacts

dotnet publish ./src/MasterDataConsumer -o ../../artifacts/MasterDataConsumer -f netcoreapp1.1 -c Docker

cp src/MasterDataConsumer/appsettings.json artifacts/MasterDataConsumer/

mkdir artifacts/logs