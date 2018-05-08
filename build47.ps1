$artifactsDir = "$PSScriptRoot/artifacts/FileAccessWebApiNet47/"
$artfifactZip = "FileAccessWebApiNet47.zip"

# Tidy up old artifacts.
Write-Host "Removing existing build artifacts..." -ForegroundColor "darkgray"

if (Test-Path -path $artifactsDir) {
    Remove-Item -Force -Recurse -Path $artifactsDir 
}

If (Test-path $artfifactZip) {
    Remove-item $artfifactZip
}

# Restore, build/publish for configuration net471.
Write-Host "Restoring .NET packages..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet restore --no-cache"

Write-Host "Publishing WebApi project..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet publish ./src/WebApi -o ../../artifacts/FileAccessWebApiNet47 -f net47"
if ($LastExitCode -ne 0) {
    throw "Publish of web api project **** Failed ****"
}

# Compress build artifacts.
Write-Host "Compressing build artifacts..." -ForegroundColor "darkgray"

Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($artifactsDir, "$PSScriptRoot/$artfifactZip")