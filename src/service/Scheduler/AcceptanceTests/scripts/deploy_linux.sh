#!/bin/bash 
cd ..
rm -rf deploy
mkdir deploy
cp Dockerfile deploy/
cp scripts/runtests.sh deploy/
cp scripts/runtestsjenkins.sh deploy/
cp scripts/wait-for-it.sh deploy/
cp scripts/rm_cr.sh deploy/
mkdir deploy/testresults

dotnet restore VSS.Productivity3D.Scheduler.AcceptanceTests.sln --no-cache

cd tests
dotnet publish RepositoryTests/RepositoryTests.csproj -o ../../deploy/RepositoryTests -f netcoreapp2.0
dotnet publish WebApiTests/WebApiTests.csproj -o ../../deploy/WebApiTests -f netcoreapp2.0