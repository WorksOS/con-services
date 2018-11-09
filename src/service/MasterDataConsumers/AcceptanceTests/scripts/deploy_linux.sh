#!/bin/bash

cd ..
rm -rf deploy
mkdir deploy
cp Dockerfile deploy/
cp scripts/runtests.sh deploy/
cp scripts/wait-for-it.sh deploy/
cp scripts/rm_cr.sh deploy/
mkdir deploy/testresults

dotnet restore --no-cache ConsumerAcceptanceTests.sln

cd tests
dotnet publish EventTests/EventTests.csproj -o ../../deploy/EventTests -f netcoreapp2.0
if [ $? -ne 0 ]; then exit 1
fi
dotnet publish KafkaTests/KafkaTests.csproj -o ../../deploy/KafkaTests -f netcoreapp2.0
if [ $? -ne 0 ]; then exit 1
fi
dotnet publish RepositoryTests/RepositoryTests.csproj -o ../../deploy/RepositoryTests -f netcoreapp2.0
if [ $? -ne 0 ]; then exit 1
fi
dotnet publish RepositoryLandfillTests/RepositoryLandfillTests.csproj -o ../../deploy/RepositoryLandfillTests -f netcoreapp2.0
if [ $? -ne 0 ]; then exit 1
fi

cp KafkaTests/appsettings.json ../deploy/KafkaTests/
cp RepositoryTests/appsettings.json ../deploy/RepositoryTests/
cp RepositoryLandfillTests/appsettings.json ../deploy/RepositoryLandfillTests/

cd ..
cd utilities
dotnet publish TestRun/TestRun.csproj -o ../../deploy/TestRun -f netcoreapp2.0
if [ $? -ne 0 ]; then exit 1
fi