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
dotnet publish WebApiTests/WebApiTests.csproj -o ..\..\deploy\WebApiTests -f netcoreapp2.0
cd ..
