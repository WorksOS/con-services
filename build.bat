RMDIR /S /Q artifacts
dotnet restore
dotnet publish ./src/MasterDataConsumer -o artifacts/MasterDataConsumer -f netcoreapp1.0 -c Docker
dotnet publish ./src/ProjectWebApi -o artifacts/ProjectWebApi -f netcoreapp1.0 -c Docker
dotnet build ./test/UnitTests/\MasterDataConsumerTests
copy src\MasterDataConsumer\appsettings.json artifacts\MasterDataConsumer\
copy src\ProjectWebApi\appsettings.json artifacts\ProjectWebApi\

mkdir artifacts\Logs
