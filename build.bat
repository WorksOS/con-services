
RMDIR /S /Q Artifacts
if exist Artifacts rd /s /q Artifacts

echo %PATH%
dotnet restore
dotnet publish ./src/WebApi -o ./Artifacts/WebApi -f net451 -c Docker
dotnet build ./test/UnitTests/WebApiTests
copy src\WebApi\appsettings.json Artifacts\WebApi\
copy src\WebApi\Dockerfile Artifacts\WebApi\

mkdir Artifacts\Logs
rem cd .\test\ComponentTests\scripts
rem deploy_win.bat

