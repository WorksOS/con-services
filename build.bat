RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/ProjectWebApi -o ../../artifacts/ProjectWebApi -f netcoreapp1.1 -c Docker
dotnet build ./test/UnitTests/MasterDataProjectTests
copy src\ProjectWebApi\appsettings.json artifacts\ProjectWebApi\

mkdir artifacts\logs
