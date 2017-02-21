RMDIR /S /Q artifacts
dotnet restore
dotnet publish ./src/Repositories -o artifacts/Repositories -f netcoreapp1.1 -c Docker
dotnet publish ./src/ConfigurationStore -o artifacts/ConfigurationStore -f netcoreapp1.1 -c Docker
dotnet publish ./src/WebApi -o artifacts/WebApi -f netcoreapp1.1 -c Docker

dotnet build ./test/UnitTests/\WebApiTests

copy src\Repositories\appsettings.json artifacts\Repositories\
copy src\ConfigurationStore\appsettings.json artifacts\ConfigurationStore\
copy src\WebApi\appsettings.json artifacts\WebApi\

mkdir artifacts\logs
