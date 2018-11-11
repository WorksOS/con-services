param ([switch] $uploadArtifact = $false)

$artifactsDir = "$PSScriptRoot/artifacts"
$artfifactZip = "MasterdataConsumerDb.zip"

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
Invoke-Expression "dotnet restore ./VSS.Productivity3D.MasterDataConsumer.sln --no-cache"

Write-Host "Publishing WebApi project..." -ForegroundColor "darkgray"
$artifactsWorkingDir = "$artifactsDir\MasterdataConsumerNet471"

Invoke-Expression "dotnet publish ./src/MasterdataConsumer/VSS.Productivity3D.MasterDataConsumer.csproj -o $artifactsWorkingDir -f net471"

# Compress build artifacts.
Write-Host "Compressing build artifacts..." -ForegroundColor "darkgray"

Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory($artifactsDir, "$PSScriptRoot/$artfifactZip") 

# Upload build artifacts to S3.
if ($uploadArtifact) {
    Write-Host "Uploading build artifacts to S3..." -ForegroundColor "darkgray"
    Invoke-Expression "aws s3 cp $artfifactZip s3://vss-merino/Productivity3D/Releases/$artfifactZip --acl public-read --profile vss-grant"
}