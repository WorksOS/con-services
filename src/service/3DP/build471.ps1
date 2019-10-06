Set-Location $PSScriptRoot

$artifactsDir = "$PSScriptRoot/artifacts"
$artifactsWorkingDir = "$artifactsDir/webapi"

Write-Host "Removing existing build artifacts..." -ForegroundColor "darkgray"

IF (Test-Path -Path $artifactsDir) { Get-ChildItem $artifactsDir -Include *.* -Recurse | Remove-Item }
IF (-NOT (Test-Path -Path "$artifactsWorkingDir/logs")) { New-Item -ItemType directory "$artifactsWorkingDir/logs" | out-null -ErrorAction Ignore }

Invoke-Expression "dotnet publish ./src/WebApi/VSS.Productivity3D.WebApi.csproj -o ../../Artifacts/WebApi -f net471"

Write-Host "Copying static deployment files..." -ForegroundColor "darkgray"

Set-Location ./src/WebApi
Copy-Item ./appsettings.json $artifactsWorkingDir
Copy-Item ./Dockerfile $artifactsWorkingDir
Copy-Item ./SetupWebAPI.ps1 $artifactsWorkingDir
Copy-Item ./SetupAwsRoute.ps1 $artifactsWorkingDir
Copy-Item ./Velociraptor.Config.Xml $artifactsWorkingDir
Copy-Item ./web.config $artifactsWorkingDir
Set-Location $PSScriptRoot
