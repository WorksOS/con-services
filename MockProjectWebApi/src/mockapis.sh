#!/bin/bash

dotnet restore --no-cache
dotnet publish  -o ./Artifacts/MockProjectWebApi -f net462 -c Docker
dotnet publish  -o ./Artifacts/MockProjectWebApi_Linux -f netcoreapp1.1 -c Docker
cp appsettings.json Artifacts/MockProjectWebApi/
cp appsettings.json Artifacts/MockProjectWebApi_Linux/
cp Dockerfile Artifacts/MockProjectWebApi/
cp Dockerfile_linux Artifacts/MockProjectWebApi_Linux/Dockerfile

