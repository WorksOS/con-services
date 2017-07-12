RMDIR /S /Q artifacts
dotnet restore --no-cache MasterDataConsumers.sln
dotnet publish ./src/MasterdataConsumer/MasterDataConsumer.csproj -o ../../artifacts/MasterdataConsumerNet47 -f net47
7z a MasterdataConsumerNet47.zip -r ./artifacts/MasterdataConsumerNet47/
