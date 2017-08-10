RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/ProjectWebApi/VSS.Project.WebApi.csproj -o ../../artifacts/ProjectWebApi -f netcoreapp1.1 -c Docker
dotnet build ./test/UnitTests/MasterDataProjectTests/VSS.Project.WebApi.Tests.csproj
copy src\ProjectWebApi\appsettings.json artifacts\ProjectWebApi\

mkdir artifacts\logs
