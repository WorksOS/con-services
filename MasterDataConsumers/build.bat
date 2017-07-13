RMDIR /S /Q artifacts
dotnet restore --no-cache MasterDataConsumers.sln
dotnet publish ./src/MasterDataConsumer/MasterDataConsumer.csproj -o ../../artifacts/MasterDataConsumer -f netcoreapp1.1 -c Docker
dotnet build ./test/UnitTests/\MasterDataConsumerTests/MasterDataConsumerTests.csproj
copy src\MasterDataConsumer\appsettings.json artifacts\MasterDataConsumer\

mkdir artifacts\logs
