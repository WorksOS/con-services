
RMDIR /S /Q Artifacts
if exist Artifacts rd /s /q Artifacts

rem echo %PATH%
nuget restore VSS.Productivity3D.Service.sln
dotnet publish ./src/TagFileHarvesterService/VSS.Productivity3D.TagfileHarvester.Service.csproj -o ../../Artifacts/TagFileHarvester -f net47 
dotnet build ./test/TagFileHarvesterTests/VSS.Productivity3D.TagFileHarvester.Tests.csproj
rem copy src\WebApi\appsettings.json Artifacts\WebApi\
rem copy src\WebApi\Dockerfile Artifacts\WebApi\
rem copy src\WebApi\SetupWebAPI.ps1 Artifacts\WebApi\
rem copy src\WebApi\Velociraptor.Config.Xml Artifacts\WebApi\
rem copy src\WebApi\web.config Artifacts\WebApi\
rem copy src\WebApi\log4net.xml Artifacts\WebApi\


mkdir Artifacts\Logs
rem cd AcceptanceTests
rem msbuild VSS.Productivity3D.Service.AcceptanceTests.sln


