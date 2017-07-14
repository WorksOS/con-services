@ECHO OFF

SET config=Release
SET projectFile=Log4Net.Extensions.csproj
SET packageServer=https://packages.vspengg.com/
SET apiKey=qATxVIHO5rIPF3K7

IF "%~1"=="" GOTO buildAndPublishPackage
IF "%~1"=="-delete" GOTO deletePackageFromServer

:buildAndPublishPackage
IF /I "%~1"=="Debug" SET config=Debug

ECHO Building [90m%projectFile%[0m for configuration: [90m%config%[0m

ECHO Removing build artifacts...
for /d /r . %%d in (obj) do @if exist "%%d" rd /s/q "%%d"

ECHO Package restore in progress...
dotnet restore %projectFile% --no-cache

ECHO Creating Nuget package...
dotnet pack %projectFile% -c %config%

ECHO Publishing package...
nuget push .\bin\release\*.nupkg %apiKey% -so %packageServer% -verbosity detailed
GOTO end

:deletePackageFromServer
IF "%~2"=="" (
  ECHO [91m"Error: No package version provided, exiting."[0m
  GOTO end
)

nuget delete log4netExtensions "%~2" -Source %packageServer% -ApiKey %apiKey%

:end
ECHO [90mFinished.[0m