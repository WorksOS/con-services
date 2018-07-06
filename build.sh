#!/bin/bash

rm -rf artifacts

dotnet publish ./src/LandfillService.WebApi.netcore/LandfillService.WebApi.netcore.csproj -o ../../artifacts/LandfillServiceWebApi --framework netcoreapp2.0 --runtime linux-x64

mkdir artifacts/logs
