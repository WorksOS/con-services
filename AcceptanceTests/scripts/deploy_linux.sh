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

dotnet restore --no-cache VSS.Productivity3D.Scheduler.AcceptanceTests.sln
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi

cd tests
dotnet publish RepositoryTests/RepositoryTests.csproj -o ../../deploy/RepositoryTests -f netcoreapp2.0
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi
dotnet publish SchedulerTestsFilterCleanup/SchedulerTestsFilterCleanup.csproj -o ../../deploy/SchedulerTestsFilterCleanup -f netcoreapp2.0
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi
dotnet publish SchedulerTestsImportedFileSync/SchedulerTestsImportedFileSync.csproj -o ../../deploy/SchedulerTestsImportedFileSync -f netcoreapp2.0
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi

cd ..
cd utilities
dotnet publish TestRun/TestRun.csproj -o ../../deploy/TestRun -f netcoreapp2.0
rc=$?; if [[ $rc != 0 ]]; then exit $rc; fi