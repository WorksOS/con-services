
RMDIR /S /Q Artifacts
if exist Artifacts rd /s /q Artifacts

mkdir Artifacts\MockProjectWebApi\

dotnet restore --no-cache MockProjectWebApi.csproj
dotnet clean MockProjectWebApi.csproj
dotnet publish MockProjectWebApi.csproj  -o ./Artifacts/MockProjectWebApi -f net471 -c Docker
if ERRORLEVEL 1 exit /b 1

copy appsettings.json Artifacts\MockProjectWebApi\
copy Dockerfile_win Artifacts\MockProjectWebApi\Dockerfile
