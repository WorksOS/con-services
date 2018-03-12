cd ..
RMDIR /S /Q deploy
mkdir deploy
copy Dockerfile deploy\
copy scripts\runtests.sh deploy\
copy scripts\wait-for-it.sh deploy\
copy scripts\rm_cr.sh deploy\
mkdir deploy\testresults

dotnet restore VSS.Productivity3D.Scheduler.AcceptanceTests.sln --no-cache

cd tests
dotnet publish RepositoryTests/RepositoryTests.csproj -o ..\..\deploy\RepositoryTests -f netcoreapp2.0
dotnet publish SchedulerTestsFilterCleanup/SchedulerTestsFilterCleanup.csproj -o ..\..\deploy\SchedulerTestsFilterCleanup -f netcoreapp2.0
dotnet publish SchedulerTestsImportedFileSync/SchedulerTestsImportedFileSync.csproj -o ..\..\deploy\SchedulerTestsImportedFileSync -f netcoreapp2.0

cd ..
cd utilities
dotnet publish TestRun/TestRun.csproj -o ..\..\deploy\TestRun -f netcoreapp2.0
cd ..