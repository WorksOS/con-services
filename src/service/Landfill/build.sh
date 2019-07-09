#!/bin/bash

rm -rf artifacts

dotnet publish ./src/LandfillService.WebApi.netcore/LandfillService.WebApi.netcore.csproj -o ../../artifacts/LandfillServiceWebApi --framework netcoreapp2.1 --runtime linux-x64

cp src/LandfillService.WebApi.netcore/appsettings.json artifacts/LandfillServiceWebApi/

mkdir artifacts/logs
