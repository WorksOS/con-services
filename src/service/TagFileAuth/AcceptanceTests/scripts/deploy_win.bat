cd ..
RMDIR /S /Q deploy
mkdir deploy
copy Dockerfile deploy\
copy scripts\runtests.sh deploy\
copy scripts\wait-for-it.sh deploy\
copy scripts\rm_cr.sh deploy\
mkdir deploy\testresults

dotnet restore --no-cache VSS.TagFileAuth.Service.AcceptanceTests.sln

cd tests
dotnet publish EventTests\EventTests.csproj -o ..\..\deploy\EventTests -f netcoreapp3.1
dotnet publish ExecutorTests\ExecutorTests.csproj -o ..\..\deploy\ExecutorTests -f netcoreapp3.1
dotnet publish IntegrationTests\IntegrationTests.csproj -o ..\..\deploy\IntegrationTests -f netcoreapp3.1
dotnet publish RepositoryTests\RepositoryTests.csproj -o ..\..\deploy\RepositoryTests -f netcoreapp3.1
dotnet publish WebApiTests\WebApiTests.csproj -o ..\..\deploy\WebApiTests -f netcoreapp3.1

copy ExecutorTests\appsettings.json ..\deploy\ExecutorTests\
copy RepositoryTests\appsettings.json ..\deploy\RepositoryTests\

cd ..
cd utilities
dotnet publish TestRun\TestRun.csproj -o ..\..\deploy\TestRun -f netcoreapp3.1
cd ..