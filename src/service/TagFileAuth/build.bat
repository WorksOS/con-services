RMDIR /S /Q artifacts

dotnet publish /nowarn:CS1591 ./src/WebApi/VSS.Productivity3D.TagFileAuth.WebAPI.csproj -o ../../artifacts/WebApi -f netcoreapp2.1 -c Docker

copy src\WebApi\appsettings.json artifacts\WebApi\
copy src\WebApi\Dockerfile artifacts\WebApi\

mkdir artifacts\logs
