RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/WebApi -o ../../artifacts/FileAccessWebApiNet47 -f net47
7z a FileAccessWebApiNet47.zip -r ./artifacts/FileAccessWebApiNet47/
