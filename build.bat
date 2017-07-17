
RMDIR /S /Q Artifacts
if exist Artifacts rd /s /q Artifacts

rem echo %PATH%
dotnet publish ./src/WebApi/VSS.Productivity3D.WebApi.csproj -o ../../Artifacts/WebApi -f net47 -c Docker
dotnet build ./test/UnitTests/WebApiTests/VSS.Productivity3D.WebApi.Tests.csproj
copy src\WebApi\appsettings.json Artifacts\WebApi\
copy src\WebApi\Dockerfile Artifacts\WebApi\
copy src\WebApi\SetupWebAPI.ps1 Artifacts\WebApi\
copy src\WebApi\Velociraptor.Config.Xml Artifacts\WebApi\
copy src\WebApi\web.config Artifacts\WebApi\
copy src\WebApi\log4net.xml Artifacts\WebApi\


mkdir Artifacts\Logs
rem cd .\test\ComponentTests\scripts
rem deploy_win.bat

