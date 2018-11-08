RMDIR /S /Q artifacts
dotnet restore --no-cache VSS.Productivity3D.MasterDataConsumer.sln
dotnet publish ./src/MasterDataConsumer/VSS.Productivity3D.MasterDataConsumer.csproj -o ../../artifacts/MasterDataConsumer -f netcoreapp2.0 -c Docker
dotnet build ./test/UnitTests/MasterDataConsumerTests/VSS.Productivity3D.MasterDataConsumer.Tests.csproj
copy src\MasterDataConsumer\appsettings.json artifacts\MasterDataConsumer\

mkdir artifacts\logs
