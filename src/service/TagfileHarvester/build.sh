#!/bin/bash

rm -rf artifacts

dotnet publish ./TagFileHarvester.netcore/TagFileHarvester.netcore.csproj -o ../artifacts/TagFileHarvester --framework netcoreapp3.1 --runtime linux-x64

