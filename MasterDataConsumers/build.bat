RMDIR /S /Q artifacts
dotnet restore
dotnet publish ./src/MasterDataConsumer -o artifacts/MasterDataConsumer -f netcoreapp1.1 -c Docker
dotnet build ./test/UnitTests/\MasterDataConsumerTests
copy src\MasterDataConsumer\appsettings.json artifacts\MasterDataConsumer\

mkdir artifacts\logs
