cd ..
RMDIR /S /Q deploy
mkdir deploy
copy Dockerfile deploy\
copy scripts\runtests.sh deploy\
copy scripts\wait-for-it.sh deploy\
copy scripts\rm_cr.sh deploy\
mkdir deploy\testresults

dotnet restore --no-cache VSS.Visionlink.Project.AcceptanceTests.sln

cd tests
dotnet publish IntegrationTests\IntegrationTests.csproj -o ..\..\deploy\IntegrationTests -f netcoreapp1.1
dotnet publish WebApiTests\WebApiTests.csproj -o ..\..\deploy\WebApiTests -f netcoreapp1.1
dotnet publish ExecutorTests\ExecutorTests.csproj -o ..\..\deploy\ExecutorTests -f netcoreapp1.1

cd ..
cd utilities
dotnet publish TestRun\TestRun.csproj -o ..\..\deploy\TestRun -f netcoreapp1.1
cd ..