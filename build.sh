#!/bin/bash

rm -rf artifacts

dotnet publish ./TagFileHarvester.netcore/TagFileHarvester.netcore.csproj -o ../artifacts/TagFileHarvester --framework netcoreapp2.0 --runtime linux-x64

