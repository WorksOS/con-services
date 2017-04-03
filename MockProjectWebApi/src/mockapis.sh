#!/bin/bash
mv project_linux.json project.json
dotnet restore --no-cache
dotnet publish -o ./Artifacts/MockProjectWebApi_linux -f netcoreapp1.1 -c Docker
cp appsettings.json Artifacts/MockProjectWebApi_linux/
cp Dockerfile_linux Artifacts/MockProjectWebApi_linux/Dockerfile

