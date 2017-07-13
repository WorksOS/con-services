#!/bin/bash
dotnet restore --no-cache src.csproj
dotnet publish src.csproj -o ./Artifacts/MockProjectWebApi -f netcoreapp1.1 -c Docker
cp appsettings.json Artifacts/MockProjectWebApi
cp Dockerfile_linux Artifacts/MockProjectWebApi/Dockerfile

