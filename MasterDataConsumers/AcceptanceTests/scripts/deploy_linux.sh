#!/bin/bash

cd ..
rm -rf deploy
mkdir deploy
cp Dockerfile deploy/
cp scripts/runtests.sh deploy/
cp scripts/wait-for-it.sh deploy/
cp scripts/rm_cr.sh deploy/
mkdir deploy/testresults

dotnet restore --no-cache

cd tests
dotnet publish EventTests/EventTests.csproj -o ../../deploy/EventTests -f netcoreapp1.1
if [ $? -ne 0 ]; then exit 1
fi
dotnet publish KafkaTests/KafkaTests.csproj -o ../../deploy/KafkaTests -f netcoreapp1.1
if [ $? -ne 0 ]; then exit 1
fi
dotnet publish RepositoryTests/RepositoryTests.csproj -o ../../deploy/RepositoryTests -f netcoreapp1.1
if [ $? -ne 0 ]; then exit 1
fi


cp KafkaTests/appsettings.json ../deploy/KafkaTests/
cp RepositoryTests/appsettings.json ../deploy/RepositoryTests/

cd ..
cd utilities
dotnet publish TestRun/TestRun.csproj -o ../../deploy/TestRun -f netcoreapp1.1
if [ $? -ne 0 ]; then exit 1
fi