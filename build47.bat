RMDIR /S /Q artifacts
dotnet restore --no-cache
dotnet publish ./src/VSS.Productivity3D.Scheduler.WebApi/VSS.Productivity3D.Scheduler.WebApi.csproj -o ../../artifacts/VSS.Productivity3D.Scheduler.WebApiNet47 -f net47
7z a VSS.Productivity3D.Scheduler.WebApiNet47.zip -r ./artifacts/VSS.Productivity3D.Scheduler.WebApiNet47/
