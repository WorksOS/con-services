RMDIR /S /Q artifacts
dotnet restore ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj --no-cache
dotnet publish ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o ../../artifacts/FilterWebApiNet47 -f netcoreapp2.0
copy src\VSS.Productivity3D.Filter.WebApi\Dockerfile_win artifacts\FilterWebApiNet47\Dockerfile
