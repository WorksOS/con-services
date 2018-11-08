cd ..
rm -rf deploy
mkdir deploy
cp Dockerfile deploy/
cp scripts/runtests.sh deploy/
cp scripts/wait-for-it.sh deploy/
cp scripts/rm_cr.sh deploy/
mkdir deploy/testresults

dotnet restore --no-cache VSS.Visionlink.Project.AcceptanceTests.sln

cd tests
dotnet publish IntegrationTests/IntegrationTests.csproj -o ../../deploy/IntegrationTests -f netcoreapp2.0
dotnet publish WebApiTests/WebApiTests.csproj -o ../../deploy/WebApiTests -f netcoreapp2.0
dotnet publish ExecutorTests/ExecutorTests.csproj -o ../../deploy/ExecutorTests -f netcoreapp2.0

