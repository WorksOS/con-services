#!/bin/bash

rm -rf artifacts

dotnet publish ./src/VSS.Tile.Service.WebApi/VSS.Tile.Service.WebApi.csproj -o ../../artifacts/VSS.Tile.Service.WebApi --framework netcoreapp2.1 --configuration Docker --runtime linux-x64

cp src/VSS.Tile.Service.WebApi/appsettings.json artifacts/VSS.Tile.Service.WebApi/

mkdir artifacts/logs
