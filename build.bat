RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/WebApi -o artifacts/WebApi -f netcoreapp1.1 -c Docker
copy src\WebApi\appsettings.json artifacts\WebApi\

mkdir artifacts\logs
