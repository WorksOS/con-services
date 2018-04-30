#!/bin/bash

rm -rf Artifacts
mkdir Artifacts/MockProjectWebApi


dotnet restore --no-cache MockProjectWebApi.csproj
dotnet publish MockProjectWebApi.csproj -o ./Artifacts/MockProjectWebApi -f netcoreapp2.0 -c Docker
cp appsettings.json Artifacts/MockProjectWebApi
cp Dockerfile_linux Artifacts/MockProjectWebApi/Dockerfile

