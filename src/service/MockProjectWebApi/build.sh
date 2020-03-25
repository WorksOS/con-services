#!/bin/bash

rm -rf artifacts

dotnet publish MockProjectWebApi.csproj -o ./artifacts/MockProjectWebApi -f netcoreapp3.1 --runtime linux-x64
mkdir artifacts/logs

