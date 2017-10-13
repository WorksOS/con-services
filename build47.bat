RMDIR /S /Q artifacts
dotnet restore --no-cache VSS.Visionlink.Project.sln
dotnet publish ./src/ProjectWebApi/VSS.Project.WebApi.csproj -o ../../artifacts/ProjectWebApiNet47 -f net47
7z a ProjectWebApiNet47.zip -r ./artifacts/ProjectWebApiNet47/
7z a ProjectWebApiDb.zip -r ./database
