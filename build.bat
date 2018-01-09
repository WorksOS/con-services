RMDIR /S /Q artifacts
dotnet restore VSS.Productivity3D.Filter.sln --no-cache
dotnet publish ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o ../../artifacts/VSS.Productivity3D.Filter.WebApi -f netcoreapp2.0 -c Docker
dotnet build ./test/UnitTests/VSS.Productivity3D.Filter.Tests/VSS.Productivity3D.Filter.Tests.csproj
copy src\VSS.Productivity3D.Filter.WebApi\appsettings.json artifacts\VSS.Productivity3D.Filter.WebApi\
copy src\VSS.Productivity3D.Filter.WebApi\Dockerfile artifacts\VSS.Productivity3D.Filter.WebApi\
copy src\VSS.Productivity3D.Filter.WebApi\bin\Docker\netcoreapp1.1\VSS.Productivity3D.Filter.WebAPI.xml artifacts\VSS.Productivity3D.Filter.WebApi\

mkdir artifacts\logs
