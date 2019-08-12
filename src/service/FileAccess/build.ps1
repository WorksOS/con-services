param ()

$artifactsDir = "$PSScriptRoot/artifacts/WebApi/"

# Tidy up old artifacts.
Write-Host "Removing existing build artifacts..." -ForegroundColor "darkgray"

if (Test-Path -path $artifactsDir) {
    Remove-Item -Force -Recurse -Path $artifactsDir 
}

Write-Host "Publishing WebApi project..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet publish /nowarn:CS1591 ./src/WebApi -o ../../artifacts/WebApi -f netcoreapp2.0 -c Docker"
if ($LastExitCode -ne 0) {
    throw "Publish of web api project **** Failed ****"
}

# Compress build artifacts.
Write-Host "Compressing build artifacts..." -ForegroundColor "darkgray"