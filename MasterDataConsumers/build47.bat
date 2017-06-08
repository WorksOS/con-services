RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/MasterdataConsumer -o ../../artifacts/MasterdataConsumerNet47 -f net47
7z a MasterdataConsumerNet47.zip -r ./artifacts/MasterdataConsumerNet47/
