#!/bin/bash

rm -rf artifacts

dotnet publish ./src/ProjectWebApi/VSS.Project.WebApi.csproj -o ../../artifacts/ProjectWebApi --framework netcoreapp2.1 --configuration Docker --runtime linux-x64

cp src/ProjectWebApi/appsettings.json artifacts/ProjectWebApi/

mkdir artifacts/logs
