RMDIR /S /Q artifacts
dotnet restore VSS.Productivity3D.Scheduler.sln --no-cache
dotnet publish ./src/VSS.Productivity3D.Scheduler.WebApi/VSS.Productivity3D.Scheduler.WebApi.csproj -o ../../artifacts/VSS.Productivity3D.Scheduler.WebApi
dotnet build ./test/UnitTests/VSS.Productivity3D.Scheduler.Tests/VSS.Productivity3D.Scheduler.Tests.csproj
copy src\VSS.Productivity3D.Scheduler.WebApi\appsettings.json artifacts\VSS.Productivity3D.Scheduler.WebApi\
copy src\VSS.Productivity3D.Scheduler.WebApi\Dockerfile artifacts\VSS.Productivity3D.Scheduler.WebApi\
copy src\VSS.Productivity3D.Scheduler.WebApi\bin\Docker\netcoreapp2.0\VSS.Productivity3D.Scheduler.WebAPI.xml artifacts\VSS.Productivity3D.Scheduler.WebApi\

mkdir artifacts\logs
