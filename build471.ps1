$artifactsDir = "artifacts"
$artfifactZip = "FilterWebApiNet47.zip"

# Tidy up old artifacts.
Write-Host "Removing existing build artifacts..." -ForegroundColor "darkgray"

if (Test-Path -path $artifactsDir) 
{
    Remove-Item -Force -Recurse -Path $artifactsDir 
}

If (Test-path $artfifactZip) 
{
    Remove-item $artfifactZip
}

# Restore, build/publish for configuration net471.
Write-Host "Restoring .NET packages..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet restore ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj --no-cache"

Write-Host "Publishing WebApi project..." -ForegroundColor "darkgray"
Invoke-Expression "dotnet publish ./src/VSS.Productivity3D.Filter.WebApi/VSS.Productivity3D.Filter.WebApi.csproj -o ../../artifacts/FilterWebApiNet47 -f net471"

$artifactsD += "/FilterWebApiNet47/"

# Copy build files to working artifacts folder.
Write-Host "Copying Docker file..." -ForegroundColor "darkgray"
if (!(Test-Path -path $artifactsD)) 
{
    New-Item $artifactsD -Type Directory
}

Copy-Item -Path "src\VSS.Productivity3D.Filter.WebApi\Dockerfile_win" -Destination $artifactsDir

# Compress build artifacts.
Write-Host "Compressing build artifacts..." -ForegroundColor "darkgray"

Add-Type -assembly "system.io.compression.filesystem"
[io.compression.zipfile]::CreateFromDirectory("$PSScriptRoot\$artifactsDir", "$PSScriptRoot\$artfifactZip") 

# Upload build artifacts to S3.
Write-Host "Uploading build artifacts to S3..." -ForegroundColor "darkgray"
Invoke-Expression "aws s3 cp $artfifactZip s3://vss-merino/Productivity3D/Releases/$artfifactZip --acl public-read --profile vss-grant"