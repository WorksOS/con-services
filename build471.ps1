param ([switch] $uploadArtifact = $false,
       [string] $artifactFilename = "FileAccess_WebApi.zip")

$artifactsDir = "$PSScriptRoot/artifacts/FileAccessWebApiNet471/"

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
Invoke-Expression "dotnet publish ./src/WebApi -o ../../artifacts/FileAccessWebApiNet471 -f net471"
if ($LastExitCode -ne 0) {
    throw "Publish of web api project **** Failed ****"
}

# Compress build artifacts.
Write-Host "Compressing build artifacts..." -ForegroundColor "darkgray"

Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($artifactsDir, "$PSScriptRoot/$artifactFilename")

# Upload build artifacts to S3.
if ($uploadArtifact) {
    Write-Host "Uploading build artifacts to S3..." -ForegroundColor "darkgray"
    Invoke-Expression "aws s3 cp $artifactFilename s3://vss-merino/Productivity3D/Releases/$artifactFilename --acl public-read --profile vss-grant"
}