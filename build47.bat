RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/ProjectWebApi -o ../../artifacts/ProjectWebApiNet47 -f net47
7z a ProjectWebApiNet47.zip -r ./artifacts/ProjectWebApiNet47/
