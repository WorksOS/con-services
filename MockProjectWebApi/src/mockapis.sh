#!/bin/bash
dotnet restore --no-cache
dotnet publish -o ./Artifacts/MockProjectWebApi -f netcoreapp1.1 -c Docker
cp appsettings.json Artifacts/MockProjectWebApi
cp Dockerfile Artifacts/MockProjectWebApi

