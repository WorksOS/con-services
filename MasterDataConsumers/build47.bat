RMDIR /S /Q artifacts
dotnet restore --no-cache VSS.Productivity3D.MasterDataConsumer.sln
dotnet publish ./src/MasterdataConsumer/VSS.Productivity3D.MasterDataConsumer.csproj -o ../../artifacts/MasterdataConsumerNet47 -f net47
7z a MasterdataConsumerNet47.zip -r ./artifacts/MasterdataConsumerNet47/
7z a MasterdataConsumerDb.zip -r ./database/
