RMDIR /S /Q artifacts
dotnet restore --no-cache VSS.TagFileAuth.Service.sln
rem dotnet publish ./src/Repositories -o artifacts/Repositories -f netcoreapp1.1 -c Docker
dotnet publish ./src/WebApi/VSS.Productivity3D.TagFileAuth.WebAPI.csproj -o ../../artifacts/WebApi -f netcoreapp1.1 -c Docker

rem dotnet build ./test/UnitTests/\WebApiTests

rem copy src\Repositories\appsettings.json artifacts\Repositories\
copy src\WebApi\appsettings.json artifacts\WebApi\
copy src\WebApi\Dockerfile artifacts\WebApi\

mkdir artifacts\logs
