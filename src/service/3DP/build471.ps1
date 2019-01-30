Set-Location $PSScriptRoot

$artifactsDir = "$PSScriptRoot/artifacts"
$artifactsWorkingDir = "$artifactsDir/webapi"

Write-Host "Removing existing build artifacts..." -ForegroundColor "darkgray"

IF (Test-Path -path $artifactsDir) {
    Remove-Item -Force -Recurse -Path $artifactsDir -ErrorAction Ignore
    New-Item -ItemType directory "$artifactsWorkingDir/logs" | out-null
}

Invoke-Expression "dotnet publish ./src/WebApi/VSS.Productivity3D.WebApi.csproj -o ../../Artifacts/WebApi -f net471"

Write-Host "Copying static deployment files..." -ForegroundColor "darkgray"

Set-Location ./src/WebApi
Copy-Item ./appsettings.json $artifactsWorkingDir
Copy-Item ./Dockerfile $artifactsWorkingDir
Copy-Item ./SetupWebAPI.ps1 $artifactsWorkingDir
Copy-Item ./Velociraptor.Config.Xml $artifactsWorkingDir
Copy-Item ./web.config $artifactsWorkingDir
Copy-Item ./log4net.xml $artifactsWorkingDir
Set-Location $PSScriptRoot
