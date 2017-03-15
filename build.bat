
RMDIR /S /Q Artifacts
if exist Artifacts rd /s /q Artifacts

rem echo %PATH%
dotnet publish ./src/WebApi -o ./Artifacts/WebApi -f net462 -c Docker
dotnet build ./test/UnitTests/WebApiTests
copy src\WebApi\appsettings.json Artifacts\WebApi\
copy src\WebApi\Dockerfile Artifacts\WebApi\

mkdir Artifacts\Logs
rem cd .\test\ComponentTests\scripts
rem deploy_win.bat

