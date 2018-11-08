RMDIR /S /Q artifacts

dotnet restore --no-cache VSS.TagFileAuth.Service.sln
dotnet publish ./src/WebApi/VSS.Productivity3D.TagFileAuth.WebAPI.csproj -o ../../artifacts/WebApi -f netcoreapp2.0 -c Docker

copy src\WebApi\appsettings.json artifacts\WebApi\
copy src\WebApi\Dockerfile artifacts\WebApi\

mkdir artifacts\logs
