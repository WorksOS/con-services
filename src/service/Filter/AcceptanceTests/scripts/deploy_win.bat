cd ..
RMDIR /S /Q deploy
mkdir deploy
copy Dockerfile deploy\
copy scripts\runtests.sh deploy\
copy scripts\wait-for-it.sh deploy\
copy scripts\rm_cr.sh deploy\
mkdir deploy\testresults

dotnet restore VSS.Visionlink.Filter.AcceptanceTests.sln --no-cache

cd tests
dotnet publish WebApiTests/WebApiTests.csproj -o ..\..\deploy\WebApiTests -f netcoreapp2.0
dotnet publish ExecutorTests/ExecutorTests.csproj -o ..\..\deploy\ExecutorTests -f netcoreapp2.0
dotnet publish RepositoryTests/RepositoryTests.csproj -o ..\..\deploy\RepositoryTests -f netcoreapp2.0
cd ..
