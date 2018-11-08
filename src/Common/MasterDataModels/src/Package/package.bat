@ECHO OFF

SET config=Release
SET projectFile=VSS.MasterData.Models.csproj
SET packageServer=https://packages.vspengg.com/
SET projectId=VSS.MasterData.Models.netcore
SET packageOutputFolder=%cd%\nupkgs
SET apiKey=qATxVIHO5rIPF3K7

IF "%~1"=="" GOTO buildAndPublishPackage
IF /I "%~1"=="--delete" GOTO deletePackageFromServer


:buildAndPublishPackage
  IF /I "%~1"=="debug" SET config=Debug

  ECHO [90mBuilding[0m [0m%projectFile%[0m [90mfor configuration:[0m [0m%config%[0m

  ECHO [90mRemoving build artifacts...[0m
  FOR /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
  rd /s /q %packageOutputFolder%

  ECHO [90mPackage restore in progress...[0m
  dotnet restore ..\%projectFile% --no-cache

  ECHO [90mCreating Nuget package...[0m
  dotnet pack --output %packageOutputFolder% ..\%projectFile% -c %config%

  ECHO [90mPublishing package...[0m
  IF /I "%~1"=="--test" (
    ECHO [33mINFO: Test mode enabled, package was not published.[0m
    GOTO end
  )

  nuget push %cd%\nupkgs\*.nupkg %apiKey% -so %packageServer% -verbosity detailed
  ECHO [90mFinished.[0m
  GOTO end


:deletePackageFromServer
  IF "%~2"=="" (
    ECHO [91mError: No package version provided, exiting.[0m
    GOTO end
  )

  nuget delete %projectId% "%~2" -Source %packageServer% -ApiKey %apiKey%


:end