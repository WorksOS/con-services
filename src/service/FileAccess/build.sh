#!/bin/bash

rm -rf artifacts

dotnet publish /nowarn:CS1591 ./src/WebApi -o ../../artifacts/WebApi -f netcoreapp2.0

mkdir artifacts/logs
