RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/WebApi -o artifacts/WebApi -f netcoreapp2.0
