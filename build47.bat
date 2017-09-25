RMDIR /S /Q artifacts
dotnet restore --no-cache VSS.TagFileAuth.Service.sln
dotnet publish ./src/WebApi/VSS.Productivity3D.TagFileAuth.WebAPI.csproj -o ../../artifacts/TagFileAuthWebApiNet47 -f net47
7z a TagFileAuthWebApiNet47.zip -r ./artifacts/TagFileAuthWebApiNet47/
