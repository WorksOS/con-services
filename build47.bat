RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/WebApi -o ../../artifacts/TagFileAuthWebApiNet47 -f net47
7z a TagFileAuthWebApiNet47.zip -r ./artifacts/TagFileAuthWebApiNet47/
