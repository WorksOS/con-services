RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/MasterdataConsumer -o ../../artifacts/MasterdataConsumer47 -f net47
7z a MasterdataConsumer47.zip -r ./artifacts/MasterdataConsumer47/
