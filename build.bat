RMDIR /S /Q artifacts
dotnet restore
dotnet publish ./src/ProjectWebApi -o artifacts/ProjectWebApi -f netcoreapp1.1 -c Docker
dotnet build ./test/UnitTests/\MasterDataConsumerTests
copy src\ProjectWebApi\appsettings.json artifacts\ProjectWebApi\

mkdir artifacts\logs
