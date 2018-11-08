RMDIR /S /Q artifacts
dotnet restore --no-cache VSS.Productivity3D.FileAccess.Service.sln
dotnet publish ./src/WebApi/VSS.Productivity3D.FileAccess.WebAPI.csproj -o ../../artifacts/WebApi -f netcoreapp2.0 -c Docker
dotnet build ./test/UnitTests/WebApiTests/WebApiTests.csproj
copy src\WebApi\appsettings.json artifacts\WebApi\

mkdir artifacts\logs
