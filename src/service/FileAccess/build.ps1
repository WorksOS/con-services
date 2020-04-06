Write-Host "Building solution" -ForegroundColor "darkgray"

$artifactsDir = "$PSScriptRoot/artifacts/"

if (Test-Path -path "$artifactsDir/WebApi/") {
    Write-Host "Removing existing build artifacts..." -ForegroundColor "darkgray"
    Get-ChildItem $artifactsDir -Recurse | Remove-Item -Recurse
}

New-Item -itemtype directory "$artifactsDir/WebApi/"

Write-Host "Publishing WebApi project..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet publish /nowarn:CS1591 ./src/WebApi -o ./artifacts/WebApi -f netcoreapp3.1 -c Docker"
