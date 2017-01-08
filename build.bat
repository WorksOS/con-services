RMDIR /S /Q artifacts
dotnet restore
dotnet publish ./src/MasterDataConsumer -o artifacts/MasterDataConsumer -f netcoreapp1.1 
dotnet publish ./src/ProjectWebApi -o artifacts/ProjectWebApi -f netcoreapp1.1 

