#!/bin/bash

rm -rf artifacts

dotnet publish ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o ../../artifacts/VSS.Productivity3D.Filter.WebApi -f netcoreapp2.0 --runtime linux-x64

mkdir artifacts/logs