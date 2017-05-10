#!/bin/bash

rm -rf artifacts

dotnet publish ./src/WebApi -o artifacts/WebApi -f netcoreapp1.1

mkdir artifacts/logs