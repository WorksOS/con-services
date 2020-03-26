#!/bin/bash

rm -rf artifacts

dotnet publish /nowarn:CS1591 ./src/WebApi -o ../../artifacts/WebApi -f netcoreapp3.1

mkdir artifacts/logs
