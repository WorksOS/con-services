#!/bin/bash

rm -rf artifacts

dotnet publish ./src/WebApi -o ../../artifacts/WebApi -f netcoreapp2.0

mkdir artifacts/logs
